using paradise.Models;
using paradise.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace paradise.Areas.Admin.Controllers
{
    public class AdminQuizsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // ======================= helpers =======================
        private string CollectModelErrors()
        {
            var errs = ModelState
                .Where(kv => kv.Value.Errors.Count > 0)
                .SelectMany(kv => kv.Value.Errors.Select(e =>
                    string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? e.Exception?.Message
                        : e.ErrorMessage))
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .Distinct()
                .ToList();

            return errs.Count == 0 ? "" : string.Join(" | ", errs);
        }

        private string Unwrap(Exception ex)
        {
            var sb = new StringBuilder();
            var i = 0;
            while (ex != null && i < 10)
            {
                sb.Append(ex.GetType().Name).Append(": ").Append(ex.Message).Append(" → ");
                ex = ex.InnerException;
                i++;
            }
            return sb.ToString();
        }

        private void DebugLog(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[AdminQuizs] " + msg);
        }

        // ======================= INDEX =======================
        public ActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(search) && Request.QueryString["search"] != null)
                return RedirectToAction("Index");

            var query = db.quizs
                .Include(q => q.topic_quiz)
                .OrderByDescending(q => q.created_at)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(q => q.title.Contains(s) || q.topic_quiz.title.Contains(s));
            }

            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Search = search;

            return View(items);
        }

        // ======================= DETAILS =======================
        public ActionResult Details(long? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var quiz = db.quizs
                .Include(q => q.topic_quiz)
                .Include(q => q.quiz_questions.Select(qq => qq.quiz_options))
                .FirstOrDefault(q => q.id == id);

            if (quiz == null) return HttpNotFound();
            return View(quiz);
        }

        // ======================= CREATE GET =======================
        public ActionResult Create()
        {
            ViewBag.topicItems = new SelectList(db.topic_quiz.OrderBy(x => x.title), "id", "title");
            return View(new QuizCreateVm
            {
                time = 30,
                is_infinity = false,
                quantity = 1
            });
        }

        // ======================= CREATE POST =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(QuizCreateVm model)
        {
            DebugLog("Create POST begin");

            if (model == null)
            {
                TempData["CustomError"] = "Dữ liệu không hợp lệ.";
                ViewBag.topicItems = new SelectList(db.topic_quiz, "id", "title");
                return View(model);
            }

            // Validate chủ đề
            if (model.topic <= 0 || !db.topic_quiz.Any(t => t.id == model.topic))
                ModelState.AddModelError("topic", "Vui lòng chọn chủ đề hợp lệ.");

            // Validate thời gian
            if (!model.is_infinity)
            {
                if (!model.time.HasValue)
                    ModelState.AddModelError("time", "Vui lòng nhập thời lượng hoặc bật 'Không giới hạn'.");
                else if (model.time.Value < 0)
                    ModelState.AddModelError("time", "Thời gian phải ≥ 0.");
            }

            // Validate câu hỏi
            if (model.Questions == null || model.Questions.Count < 1)
                ModelState.AddModelError("Questions", "Phải có ít nhất 1 câu hỏi.");
            else
            {
                for (int i = 0; i < model.Questions.Count; i++)
                {
                    var q = model.Questions[i];
                    if (string.IsNullOrWhiteSpace(q.question_text))
                        ModelState.AddModelError($"Questions[{i}].question_text", "Nội dung câu hỏi không được trống.");

                    if (q.Options == null || q.Options.Count < 2)
                        ModelState.AddModelError($"Questions[{i}].Options", "Mỗi câu cần ít nhất 2 đáp án.");
                    else
                    {
                        for (int j = 0; j < q.Options.Count; j++)
                        {
                            if (string.IsNullOrWhiteSpace(q.Options[j]?.option_text))
                                ModelState.AddModelError($"Questions[{i}].Options[{j}].option_text", $"Đáp án {j + 1} không được trống.");
                        }
                    }

                    if (!q.CorrectOption.HasValue ||
                        q.CorrectOption.Value < 0 ||
                        q.CorrectOption.Value >= (q.Options?.Count ?? 0))
                        ModelState.AddModelError($"Questions[{i}].CorrectOption", "Phải chọn 1 đáp án đúng.");
                }
            }

            model.quantity = model.Questions?.Count ?? 0;

            if (!ModelState.IsValid)
            {
                TempData["CustomError"] = "Form chưa hợp lệ: " + CollectModelErrors();
                ViewBag.topicItems = new SelectList(db.topic_quiz, "id", "title", model.topic);
                return View(model);
            }

            // SAVE TO DB
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var quiz = new quiz
                    {
                        title = model.title?.Trim(),
                        topic = model.topic,
                        time = model.is_infinity ? (decimal?)null : model.time,
                        is_infinity = model.is_infinity,
                        quantity = model.quantity,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    };

                    db.quizs.Add(quiz);
                    db.SaveChanges();

                    foreach (var qvm in model.Questions)
                    {
                        var q = new quiz_questions
                        {
                            quiz_id = quiz.id,
                            question_text = qvm.question_text?.Trim(),
                            created_at = DateTime.Now
                        };
                        db.quiz_questions.Add(q);
                        db.SaveChanges();

                        for (int idx = 0; idx < qvm.Options.Count; idx++)
                        {
                            var optVm = qvm.Options[idx];
                            var opt = new quiz_options
                            {
                                question_id = q.id,
                                option_text = optVm.option_text?.Trim(),
                                is_correct = (idx == qvm.CorrectOption)
                            };
                            db.quiz_options.Add(opt);
                        }
                        db.SaveChanges();
                    }

                    tx.Commit();
                    TempData["ToastSuccess"] = "Tạo quiz và câu hỏi thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["CustomError"] = "Lỗi: " + Unwrap(ex);
                }
            }

            ViewBag.topicItems = new SelectList(db.topic_quiz, "id", "title", model.topic);
            return View(model);
        }

        // ======================= EDIT (GET) =======================
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.quizs
                .Include(q => q.quiz_questions.Select(qq => qq.quiz_options))
                .FirstOrDefault(q => q.id == id);

            if (entity == null) return HttpNotFound();

            var vm = new QuizEditVm
            {
                id = entity.id,
                title = entity.title,
                topic = entity.topic,
                time = entity.time,
                is_infinity = entity.is_infinity,
                quantity = entity.quiz_questions.Count(),
                Questions = entity.quiz_questions
                    .OrderBy(qq => qq.id)
                    .Select(qq => new QuestionEditVm
                    {
                        id = qq.id,
                        question_text = qq.question_text,
                        Options = qq.quiz_options
                            .OrderBy(o => o.id)
                            .Select(o => new OptionEditVm
                            {
                                id = o.id,
                                option_text = o.option_text
                            }).ToList(),
                        CorrectOption = qq.quiz_options
                            .OrderBy(o => o.id)
                            .Select((o, idx) => new { o, idx })
                            .FirstOrDefault(x => x.o.is_correct)?.idx
                    })
                    .ToList()
            };

            // Tạo list SelectListItem để tránh lỗi long/int mismatch
            ViewBag.topicItems = db.topic_quiz
                .OrderBy(x => x.title)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.title,
                    Selected = x.id == vm.topic
                }).ToList();

            return View(vm);
        }

        // ======================= EDIT (POST) =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(QuizEditVm model)
        {
            if (model == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (model.topic <= 0 || !db.topic_quiz.Any(t => t.id == model.topic))
                ModelState.AddModelError("topic", "Chủ đề không hợp lệ.");

            if (!model.is_infinity)
            {
                if (!model.time.HasValue)
                    ModelState.AddModelError("time", "Vui lòng nhập thời lượng.");
                else if (model.time.Value < 0)
                    ModelState.AddModelError("time", "Thời lượng phải ≥ 0.");
            }

            var entity = db.quizs
                .Include(q => q.quiz_questions.Select(qq => qq.quiz_options))
                .FirstOrDefault(q => q.id == model.id);
            if (entity == null) return HttpNotFound();

            var formQs = model.Questions ?? new List<QuestionEditVm>();
            var effectiveQs = formQs.Where(q => q._remove != true).ToList();
            if (effectiveQs.Count < 1)
                ModelState.AddModelError("Questions", "Phải có ít nhất 1 câu hỏi.");

            if (!ModelState.IsValid)
            {
                ModelState.Remove("topic");
                ViewBag.topicItems = db.topic_quiz.OrderBy(x => x.title)
                    .ToList()
                    .Select(x => new SelectListItem
                    {
                        Value = x.id.ToString(),
                        Text = x.title,
                        Selected = x.id == model.topic
                    }).ToList();
                return View(model);
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    entity.title = model.title?.Trim();
                    entity.topic = model.topic;
                    entity.is_infinity = model.is_infinity;
                    entity.time = model.is_infinity ? (decimal?)null : model.time;
                    entity.updated_at = DateTime.Now;

                    // --- Đồng bộ câu hỏi & đáp án ---
                    var formQIdsKeep = formQs.Where(q => q._remove != true && q.id.HasValue)
                                             .Select(q => q.id.Value)
                                             .ToHashSet();

                    var deleteQs = entity.quiz_questions
                        .Where(qq => !formQIdsKeep.Contains(qq.id) ||
                                     formQs.Any(fq => fq.id == qq.id && fq._remove == true))
                        .ToList();

                    foreach (var del in deleteQs)
                    {
                        db.quiz_options.RemoveRange(del.quiz_options);
                        db.quiz_questions.Remove(del);
                    }

                    foreach (var qvm in formQs)
                    {
                        if (qvm._remove == true) continue;

                        quiz_questions qq;
                        if (qvm.id.HasValue && qvm.id.Value > 0)
                        {
                            qq = entity.quiz_questions.First(x => x.id == qvm.id.Value);
                            qq.question_text = qvm.question_text?.Trim();
                        }
                        else
                        {
                            qq = new quiz_questions
                            {
                                quiz_id = entity.id,
                                question_text = qvm.question_text?.Trim(),
                                created_at = DateTime.Now
                            };
                            db.quiz_questions.Add(qq);
                            db.SaveChanges();
                        }

                        var formOpts = qvm.Options ?? new List<OptionEditVm>();
                        var keepOptIds = formOpts.Where(o => o._remove != true && o.id.HasValue)
                                                 .Select(o => o.id.Value)
                                                 .ToHashSet();

                        var toDelOpts = qq.quiz_options
                            .Where(o => !keepOptIds.Contains(o.id) ||
                                        formOpts.Any(fo => fo.id == o.id && fo._remove == true))
                            .ToList();
                        db.quiz_options.RemoveRange(toDelOpts);

                        for (int idx = 0; idx < formOpts.Count; idx++)
                        {
                            var ovm = formOpts[idx];
                            if (ovm._remove == true) continue;

                            if (ovm.id.HasValue && ovm.id.Value > 0)
                            {
                                var opt = qq.quiz_options.First(o => o.id == ovm.id.Value);
                                opt.option_text = ovm.option_text?.Trim();
                            }
                            else
                            {
                                db.quiz_options.Add(new quiz_options
                                {
                                    question_id = qq.id,
                                    option_text = ovm.option_text?.Trim(),
                                    is_correct = false
                                });
                            }
                        }

                        db.SaveChanges();

                        var ordered = qq.quiz_options.OrderBy(o => o.id).ToList();
                        int correctIdx = qvm.CorrectOption ?? -1;
                        for (int i = 0; i < ordered.Count; i++)
                            ordered[i].is_correct = (i == correctIdx);
                    }

                    entity.quantity = db.quiz_questions.Count(x => x.quiz_id == entity.id);

                    db.SaveChanges();
                    tx.Commit();

                    TempData["ToastSuccess"] = "Cập nhật quiz thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["CustomError"] = "Lỗi khi cập nhật: " + Unwrap(ex);
                }
            }

            ModelState.Remove("topic");
            ViewBag.topicItems = db.topic_quiz.OrderBy(x => x.title)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.title,
                    Selected = x.id == model.topic
                }).ToList();
            return View(model);
        }

        // ======================= DELETE =======================
        public ActionResult Delete(long? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var quiz = db.quizs.Include(q => q.topic_quiz).FirstOrDefault(q => q.id == id);
            if (quiz == null) return HttpNotFound();
            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var quiz = db.quizs.Find(id);
                    if (quiz == null) return HttpNotFound();

                    var questionIds = db.quiz_questions.Where(q => q.quiz_id == id).Select(q => q.id).ToList();
                    if (questionIds.Any())
                    {
                        db.quiz_options.RemoveRange(db.quiz_options.Where(o => questionIds.Contains(o.question_id)));
                        db.quiz_questions.RemoveRange(db.quiz_questions.Where(q => q.quiz_id == id));
                    }

                    db.quizs.Remove(quiz);
                    db.SaveChanges();

                    tx.Commit();
                    TempData["ToastSuccess"] = "Đã xoá quiz.";
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["CustomError"] = "Không thể xoá quiz: " + Unwrap(ex);
                }
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
