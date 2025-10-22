// ======= course-detail.js (gate by video end, no checkboxes) =======

// Data từ Razor
var COURSE_DATA = window.COURSE_DATA || [];
var COURSE_KEY = window.COURSE_KEY || "course:demo";
var CURRENT_LESSON_ID = window.CURRENT_LESSON_ID || "";

// State trong localStorage
var state = JSON.parse(localStorage.getItem(COURSE_KEY) || '{"done":[],"current":""}');
if (!state || !Array.isArray(state.done)) state = { done: [], current: "" };

// Cho phép reset khi bật cờ (chỉ dùng khi muốn bắt đầu lại 0%)
if (window.FRESH_PROGRESS) {
  state = { done: [], current: "" };
  localStorage.setItem(COURSE_KEY, JSON.stringify(state));
}


// Elements
var videoEl = document.getElementById("lessonVideo");
var videoSrcEl = document.getElementById("videoSource");
var contentEl = document.getElementById("lessonContent");
var quizEl = document.getElementById("quizContainer");

var playerBox = document.getElementById("player");
var ratioBox = playerBox ? playerBox.querySelector(".ratio") : null;
var ytPlayer = null;
var ytApiReady = false;

// Lấy videoId từ url YouTube
function getYouTubeId(url) {
    if (!url) return "";
    var m = url.match(/(?:v=|\/)([0-9A-Za-z_-]{11})(?:\?|&|$)/);
    return m ? m[1] : ""; // chấp nhận cả dạng youtu.be/ID hay watch?v=ID
}

// Load IFrame API 1 lần
function ensureYTApi(cb) {
    if (ytApiReady) return cb();
    var tag = document.createElement('script');
    tag.src = "https://www.youtube.com/iframe_api";
    window.onYouTubeIframeAPIReady = function () { ytApiReady = true; cb(); };
    document.head.appendChild(tag);
}

// Xoá mọi nội dung trong .ratio (iframe cũ)
function clearRatio() {
    if (!ratioBox) return;
    // Ẩn <video>
    if (videoEl) { videoEl.pause(); videoEl.src = ""; videoEl.classList.remove("d-block"); videoEl.classList.add("d-none"); }
    // Gỡ iframe YT nếu có
    var ifr = ratioBox.querySelector("iframe");
    if (ifr && ifr.parentNode) ifr.parentNode.removeChild(ifr);
    // Destroy ytPlayer
    if (ytPlayer && ytPlayer.destroy) { try { ytPlayer.destroy(); } catch (e) { } ytPlayer = null; }
    // Dọn nội dung #lessonContent
    contentEl.innerHTML = "";
}

// Hiển thị thẻ video <video>
function mountVideo(lesson) {
    clearRatio();
    playerBox.style.display = "";            // hiện khung player
    // show <video>
    videoEl.classList.remove("d-none");
    videoEl.classList.add("d-block");

    videoSrcEl.src = lesson.videoSrc || "";
    videoEl.load();
    videoEl.onended = function () { completeCurrentIfNeeded(); };
}

// Hiển thị iframe YouTube
function mountYouTube(lesson) {
    clearRatio();
    playerBox.style.display = "";            // hiện khung player

    var vid = getYouTubeId(lesson.youtubeUrl || "");
    if (!vid) { contentEl.innerHTML = "<div class='alert alert-warning'>YouTube URL không hợp lệ.</div>"; return; }

    // Ẩn <video>
    videoEl.classList.add("d-none");

    ensureYTApi(function () {
        var host = document.createElement("div");
        host.id = "ytHost";
        host.style.width = "100%";
        host.style.height = "100%";
        ratioBox.appendChild(host);

        ytPlayer = new YT.Player('ytHost', {
            videoId: vid,
            playerVars: { rel: 0, modestbranding: 1, controls: 1 },
            events: {
                onStateChange: function (e) {
                    if (e.data === YT.PlayerState.ENDED) completeCurrentIfNeeded();
                }
            }
        });
    });
}

// Hiển thị bài dạng text (ẩn khung video)
function mountText(lesson) {
    clearRatio();
    playerBox.style.display = "none";        // ẩn khung player
    contentEl.innerHTML = lesson.contentHtml || "<p>(Chưa có nội dung)</p>";

    // Tuỳ chọn: thêm nút đánh dấu hoàn thành
    var btn = document.createElement("button");
    btn.className = "btn btn-success mt-2";
    btn.textContent = "Đánh dấu hoàn thành";
    btn.onclick = function () { completeCurrentIfNeeded(); };
    contentEl.appendChild(btn);
}



// ===== Helpers
function flatLessons(data) {
    var out = [];
    for (var si = 0; si < data.length; si++) {
        var sec = data[si] || { lessons: [] };
        for (var li = 0; li < sec.lessons.length; li++) {
            var l = sec.lessons[li];
            out.push({
                si: si, li: li,
                id: l.id,
                title: l.title,
                duration: l.duration,
                videoSrc: l.videoSrc,
                contentHtml: l.contentHtml,
                quiz: l.quiz || []
            });
        }
    }
    return out;
}
var FLAT = flatLessons(COURSE_DATA);

function idxOf(id) {
    for (var i = 0; i < FLAT.length; i++) if (FLAT[i].id === id) return i;
    return -1;
}
function saveState() { localStorage.setItem(COURSE_KEY, JSON.stringify(state)); }

// Tính “đã xong đến đâu”
function lastDoneIdx() {
    var last = -1;
    for (var i = 0; i < FLAT.length; i++) {
        if (state.done.indexOf(FLAT[i].id) >= 0) last = i;
        else break; // chỉ tính liên tiếp từ đầu
    }
    return last;
}
// Một bài có được mở chưa?
function isUnlocked(i) { return i <= lastDoneIdx() + 1; }

function canOpenByFlatIndex(i) {
    if (i < 0) return false;
    if (i === 0) return true;                       // bài đầu luôn mở
    return state.done.indexOf(FLAT[i - 1].id) >= 0;   // mở khi bài trước đã xong
}

function svgCheck(cls) {
    return '<svg class="' + (cls || '') + '" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M6.2 11.2L3.5 8.5l1.4-1.4 1.3 1.3L11 3.6l1.4 1.4-6.2 6.2z" fill="currentColor"/></svg>';
}
function svgLock() {
    return '<svg class="text-muted" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M4 7V5a4 4 0 1 1 8 0v2h1a1 1 0 0 1 1 1v6a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V8a1 1 0 0 1 1-1h1zm2-2v2h4V5a2 2 0 1 0-4 0z" fill="currentColor"/></svg>';
}
function svgDot() {
    return '<span class="text-muted" style="font-size:18px;line-height:1">•</span>';
}

function redrawSidebars() {
    var cur = state.current;
    renderSidebar("sidebar");
    renderSidebar("sidebar-mobile");
    // highlight lại bài đang mở
    document.querySelectorAll('.lesson[data-lesson-id="' + cur + '"]').forEach(function (el) {
        el.classList.add('active');
    });
}

function renderSidebar(targetId) {
    var container = document.getElementById(targetId);
    if (!container) return;

    var html = '<div class="sidebar-card">';

    for (var si = 0; si < COURSE_DATA.length; si++) {
        var sec = COURSE_DATA[si] || { lessons: [] };
        var learned = 0;
        for (var k = 0; k < sec.lessons.length; k++) {
            if (state.done.indexOf(sec.lessons[k].id) >= 0) learned++;
        }

        var secId = 'sec-' + targetId + '-' + si;
        var showClass = (window.innerWidth < 992) ? '' : 'show';  // mobile đóng, desktop mở

        html += '<div class="card-section">';
        html += '<div class="card-header d-flex justify-content-between align-items-center">';
        html += '<button class="btn-sec-toggle collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#' + secId + '" aria-controls="' + secId + '">';
        html += '<svg class="chev" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M5.2 2.8l5.6 5.2-5.6 5.2" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';
        html += '<span>' + sec.title + '</span>';
        html += '</button>';
        html += '<small class="text-muted" id="sec-progress-' + si + '">(' + learned + '/' + sec.lessons.length + ' bài đã học)</small>';
        html += '</div>';

        html += '<ul id="' + secId + '" class="list-group list-group-flush collapse ' + showClass + '">';

        for (var li = 0; li < sec.lessons.length; li++) {
            var l = sec.lessons[li];
            var done = state.done.indexOf(l.id) >= 0;
            var gIdx = idxOf(l.id);
            var canOpen = done || canOpenByFlatIndex(gIdx);  // đã xong => luôn mở; chưa xong => mở nếu bài trước xong

            var iconHtml = done ? svgCheck('text-success') : (canOpen ? svgDot() : svgLock());
            var disabledClass = (canOpen ? '' : ' disabled');

            html += '<li class="list-group-item d-flex align-items-center lesson' + disabledClass + '" data-lesson-id="' + l.id + '">';
            html += iconHtml;
            html += '<span class="flex-grow-1 lesson-title ms-2">' + l.title + '</span>';
            html += '<small class="text-muted ms-2">' + (l.duration || '') + '</small>';
            html += '</li>';
        }

        html += '</ul>';
        html += '</div>';
    }

    html += '</div>';
    container.innerHTML = html;

    // Click bài học
    container.querySelectorAll(".lesson").forEach(function (row) {
        row.addEventListener("click", function () {
            if (this.classList.contains("disabled")) return; // khoá => không mở
            var id = this.getAttribute("data-lesson-id");
            openLessonByIndex(idxOf(id));
        });
    });
}



// ===== Nội dung + Quiz (giữ như cũ, có thể chấm điểm bổ sung)
function renderContentAndQuiz() {
}

// ===== Đánh dấu hoàn thành bài hiện tại
function completeCurrentIfNeeded() {
    var id = state.current;
    if (!id) return;
    if (state.done.indexOf(id) < 0) {
        state.done.push(id);
        saveState();
        updateSectionCounts();
        updateTotalPercent();
        renderSidebar("sidebar");
        renderSidebar("sidebar-mobile");
        syncBottomBar();
    }
}

// ===== Mở bài
function openLessonByIndex(newIdx) {
    if (newIdx < 0 || newIdx >= FLAT.length) return;
    if (!isUnlocked(newIdx)) return; // chặn mở khóa khi chưa đủ điều kiện

    var lesson = FLAT[newIdx];
    state.current = lesson.id;
    saveState(); // lưu current

    // Gán video
    videoSrcEl.src = lesson.videoSrc || "";
    if (videoEl) {
        videoEl.load();
        // Khi xem hết video => hoàn thành bài
        videoEl.onended = function () {
            completeCurrentIfNeeded();
            // Sau khi hoàn thành, tự enable nút Next; có thể tự nhảy sang bài kế tiếp nếu muốn
            syncBottomBar();
        };
    }

    //var t = (lesson.type || "video").toLowerCase();
    //if (t === "youtube" || t === "link" || t === "yt") {
    //    mountYouTube(lesson);
    //} else if (t === "text") {
    //    mountText(lesson);
    //} else {
    //    mountVideo(lesson);
    //}

    renderContentAndQuiz(lesson);

    // Active dòng hiện tại
    var all = document.querySelectorAll(".lesson");
    for (var i = 0; i < all.length; i++) all[i].classList.remove("active");
    var active = document.querySelector('.lesson[data-lesson-id="' + lesson.id + '"]');
    if (active) active.classList.add("active");

    updateSectionCounts();
    updateTotalPercent();
    syncBottomBar();

    if (window.innerWidth < 992) {
        var ratio = document.querySelector(".ratio");
        if (ratio) ratio.scrollIntoView({ behavior: "smooth", block: "start" });
    }
}

// ===== Bottom bar
var bbPrev = document.getElementById("bb-prev");
var bbNext = document.getElementById("bb-next");
var bbTitle = document.getElementById("bb-title");

function syncBottomBar() {
    var i = idxOf(state.current);
    var cur = FLAT[i] || FLAT[0];
    bbTitle.textContent = cur ? (i + 1) + ". " + cur.title : "—";

    if (bbPrev) bbPrev.disabled = i <= 0;
    if (bbNext) {
        var canNext = (i >= 0) && isUnlocked(i + 1); // chỉ bật khi bài kế tiếp đã được mở (tức đã hoàn thành bài hiện tại)
        bbNext.disabled = !(canNext);
    }
}
if (bbPrev) bbPrev.addEventListener("click", function () {
    var i = idxOf(state.current);
    openLessonByIndex(i - 1);
});
if (bbNext) bbNext.addEventListener("click", function () {
    var i = idxOf(state.current);
    openLessonByIndex(i + 1);
});
window.addEventListener("keydown", function (e) {
    if (["INPUT", "TEXTAREA"].indexOf(e.target.tagName) >= 0) return;
    var i = idxOf(state.current);
    if (e.key === "ArrowLeft") { e.preventDefault(); if (bbPrev && !bbPrev.disabled) bbPrev.click(); }
    if (e.key === "ArrowRight") {
        e.preventDefault(); if (bbNext && !bbNext.disabled) {
            // chỉ cho đi tiếp khi đã unlock
            if (isUnlocked(i + 1)) bbNext.click();
        }
    }
});

// ===== Progress
function updateSectionCounts() {
    for (var si = 0; si < COURSE_DATA.length; si++) {
        var ids = COURSE_DATA[si].lessons.map(function (x) { return x.id; });
        var learned = 0;
        for (var k = 0; k < state.done.length; k++) if (ids.indexOf(state.done[k]) >= 0) learned++;
        var el = document.getElementById("sec-progress-" + si);
        if (el) el.textContent = "(" + learned + "/" + COURSE_DATA[si].lessons.length + " bài đã học)";
    }
}
function updateTotalPercent() {
    var total = FLAT.length, learned = state.done.length;
    var pct = total ? Math.round((learned / total) * 100) : 0;
    var ring = document.getElementById("ring");
    if (ring) {
        ring.style.setProperty("--val", pct);
        var span = ring.querySelector("span");
        if (span) span.textContent = pct + "%";
    }
}

localStorage.removeItem(COURSE_KEY);
// ===== Init
(function init() {
    renderSidebar("sidebar");
    renderSidebar("sidebar-mobile");

    if (!state.current) state.current = CURRENT_LESSON_ID || (FLAT[0] ? FLAT[0].id : "");
    var i = idxOf(state.current);
    if (i < 0) i = 0;

    openLessonByIndex(i);
})();



    (function(){
    var layout = document.getElementById('learnLayout');
    var fab    = document.getElementById('btnToggleSidebar');
    var KEY    = (window.COURSE_KEY || 'course:demo') + ':sidebarCollapsed';

    // Khôi phục trạng thái lần trước
    try {
      if (localStorage.getItem(KEY) === '1') layout.classList.add('is-collapsed');
    } catch(e){ }

    // Toggle khi bấm nút 3 gạch (chỉ desktop/tablet)
    if (fab) {
        fab.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                // Trên mobile, mở offcanvas thay vì co lưới
                var el = document.getElementById('lessonDrawer');
                if (el && window.bootstrap) new bootstrap.Offcanvas(el).toggle();
                return;
            }
            layout.classList.toggle('is-collapsed');
            try { localStorage.setItem(KEY, layout.classList.contains('is-collapsed') ? '1' : '0'); } catch (e) { }
        });
    }
  })();



