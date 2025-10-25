
// Data từ Razor
var COURSE_DATA = window.COURSE_DATA || [];
var COURSE_KEY = window.COURSE_KEY || "course:demo";
var CURRENT_LESSON_ID = window.CURRENT_LESSON_ID || "";

// State
var state = JSON.parse(localStorage.getItem(COURSE_KEY) || '{"done":[],"current":""}');
if (!state || !Array.isArray(state.done)) state = { done: [], current: "" };
if (window.FRESH_PROGRESS) {
    state = { done: [], current: "" };
    localStorage.setItem(COURSE_KEY, JSON.stringify(state));
}

// Elements
var playerBox = document.getElementById("player");
var ratioBox = playerBox ? playerBox.querySelector(".ratio") : null;
var controlsEl = document.getElementById("vControls");
var contentEl = document.getElementById("lessonContent");

var videoEl = document.getElementById("lessonVideo");
var videoSrcEl = document.getElementById("videoSource");

// --- YouTube overlay container (overlay lên <video>) ---
var ytHost = null;
if (ratioBox) {
    ytHost = document.createElement("iframe");
    ytHost.id = "ytFrame";
    ytHost.style.cssText = "position:absolute;inset:0;border:0;border-radius:10px;display:none;width:100%;height:100%";
    ytHost.setAttribute("allow", "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share");
    ytHost.setAttribute("allowfullscreen", "allowfullscreen");
    ratioBox.style.position = "relative";
    ratioBox.appendChild(ytHost);
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
                kind: (l.kind || "video").toLowerCase(),      // "video" | "youtube" | "text"
                videoSrc: l.videoSrc || "",
                youtubeId: l.youtubeId || "",
                contentHtml: l.contentHtml || "",
                quiz: l.quiz || []
            });
        }
    }
    return out;
}
var FLAT = flatLessons(COURSE_DATA);

function idxOf(id) { for (var i = 0; i < FLAT.length; i++) if (FLAT[i].id === id) return i; return -1; }
function saveState() { localStorage.setItem(COURSE_KEY, JSON.stringify(state)); }
function lastDoneIdx() { var last = -1; for (var i = 0; i < FLAT.length; i++) { if (state.done.indexOf(FLAT[i].id) >= 0) last = i; else break; } return last; }
function isUnlocked(i) { return i <= lastDoneIdx() + 1; }
function canOpenByFlatIndex(i) { if (i < 0) return false; if (i === 0) return true; return state.done.indexOf(FLAT[i - 1].id) >= 0; }
function cacheBust(u) { if (!u) return ""; return u + (u.indexOf("?") >= 0 ? "&" : "?") + "t=" + Date.now(); }

// ===== Nút "Đánh dấu hoàn thành" (ở bottom bar, chỉ Text & YouTube)
var btnMarkDone = null;
function ensureMarkDoneButton() {
    if (btnMarkDone) return btnMarkDone;
    btnMarkDone = document.createElement('button');
    btnMarkDone.id = 'bb-markdone';
    btnMarkDone.type = 'button';
    btnMarkDone.className = 'btn btn-outline-success me-2';
    btnMarkDone.textContent = 'Đánh dấu hoàn thành';
    var bbNext = document.getElementById('bb-next');
    if (bbNext && bbNext.parentElement) {
        bbNext.parentElement.insertBefore(btnMarkDone, bbNext);
    } else {
        document.body.appendChild(btnMarkDone);
    }
    btnMarkDone.onclick = function () {
        completeCurrentIfNeeded();
        updateMarkDoneVisibility(); // ẩn ngay sau khi complete
    };
    return btnMarkDone;
}
function updateMarkDoneVisibility(lesson) {
    var btn = ensureMarkDoneButton();
    if (!lesson) {
        var i = idxOf(state.current);
        lesson = FLAT[i];
    }
    var already = state.done.indexOf(lesson.id) >= 0;
    var showForType = (lesson.kind === 'text') || (lesson.kind === 'youtube');
    btn.style.display = (!already && showForType) ? '' : 'none';
}

// ===== Reset/hiển thị player
function resetVideo() {
    if (!videoEl) return;
    try { videoEl.pause(); } catch (e) { }
    try { videoEl.currentTime = 0; } catch (e) { }
    videoEl.onended = null;
    videoEl.removeAttribute("src");
    if (videoSrcEl) videoSrcEl.removeAttribute("src");
    var sources = videoEl.querySelectorAll("source");
    for (var i = 1; i < sources.length; i++) { try { sources[i].parentNode.removeChild(sources[i]); } catch (e) { } }
    try { videoEl.load(); } catch (e) { }
}
function hideYouTube() {
    if (!ytHost) return;
    ytHost.style.display = "none";
    ytHost.removeAttribute("src");
}
function showYouTube(lesson) {
    resetVideo();
    playerBox.style.display = "";
    if (controlsEl) controlsEl.style.display = "none";
    if (ytHost) {
        ytHost.src = "https://www.youtube.com/embed/" + (lesson.youtubeId || "") + "?rel=0";
        ytHost.style.display = "block";
    }
}
function showVideo(lesson) {
    hideYouTube();
    playerBox.style.display = "";
    if (controlsEl) controlsEl.style.display = "";
    resetVideo();
    var url = cacheBust(lesson.videoSrc || "");
    if (videoSrcEl) videoSrcEl.src = url;
    videoEl.src = url;
    try { videoEl.load(); } catch (e) { }
    videoEl.onended = function () { completeCurrentIfNeeded(); };
}
function showText(lesson) {
    resetVideo();
    hideYouTube();
    playerBox.style.display = "none";
    if (controlsEl) controlsEl.style.display = "";
}

// ===== Sidebar & progress
function svgCheck(cls) { return '<svg class="' + (cls || '') + '" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M6.2 11.2L3.5 8.5l1.4-1.4 1.3 1.3L11 3.6l1.4 1.4-6.2 6.2z" fill="currentColor"/></svg>'; }
function svgLock() { return '<svg class="text-muted" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M4 7V5a4 4 0 1 1 8 0v2h1a1 1 0 0 1 1 1v6a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V8a1 1 0 0 1 1-1h1zm2-2v2h4V5a2 2 0 1 0-4 0z" fill="currentColor"/></svg>'; }
function svgDot() { return '<span class="text-muted" style="font-size:18px;line-height:1">•</span>'; }


// ===== Search: helpers =====
var SEARCH_KEY = (COURSE_KEY || 'course:demo') + ':lsSearch';
function saveSearch(q) { try { localStorage.setItem(SEARCH_KEY, q || ''); } catch (e) { } }
function loadSearch() { try { return localStorage.getItem(SEARCH_KEY) || ''; } catch (e) { return ''; } }

// bỏ dấu tiếng Việt + lower
function vnNorm(s) {
    if (!s) return '';
    return s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
}
// bôi vàng chuỗi khớp (không dấu)
function highlightHit(text, q) {
    if (!q) return text;
    var t = text || '';
    var base = vnNorm(t);
    var k = vnNorm(q);
    var i = base.indexOf(k);
    if (i < 0) return t;
    // map index từ base -> original (đơn giản hoá: cắt theo i và độ dài k ở original)
    // để an toàn, bọc theo substring cùng độ dài trên chuỗi gốc
    var start = i, end = i + k.length;
    return t.substring(0, start) + '<mark class="ls-hit">' + t.substring(start, end) + '</mark>' + t.substring(end);
}

// === Duration cache
var DUR_KEY = (window.COURSE_KEY || "course:demo") + ":dur";
var durCache = {};
try { durCache = JSON.parse(localStorage.getItem(DUR_KEY) || "{}"); } catch (e) { }
function saveDurCache() { try { localStorage.setItem(DUR_KEY, JSON.stringify(durCache)); } catch (e) { } }

function fmtTime(sec) {
    sec = Math.max(0, Math.round(sec || 0));
    var h = Math.floor(sec / 3600);
    var m = Math.floor((sec % 3600) / 60);
    var s = sec % 60;
    function d(n) { return (n < 10 ? "0" : "") + n; }
    return h > 0 ? (d(h) + ":" + d(m) + ":" + d(s)) : (d(m) + ":" + d(s));
}

// --- đo thời lượng file video (mp4, webm, ...)
function probeVideoDuration(url, cb) {
    var v = document.createElement("video");
    v.preload = "metadata";
    v.muted = true;
    v.src = url; // nếu backend có cache-bust thì dùng luôn function cacheBust(url)
    v.onloadedmetadata = function () {
        var secs = v.duration || 0;
        cleanup(); cb(secs);
    };
    v.onerror = function () { cleanup(); cb(0); };
    function cleanup() {
        try { v.removeAttribute("src"); v.load(); } catch (e) { }
    }
}

// --- tải YouTube Iframe API 1 lần
var _ytWaiting = null;
function loadYTApi(done) {
    if (window.YT && window.YT.Player) { done(); return; }
    if (_ytWaiting) { _ytWaiting.push(done); return; }
    _ytWaiting = [done];
    var s = document.createElement("script");
    s.src = "https://www.youtube.com/iframe_api";
    document.head.appendChild(s);
    window.onYouTubeIframeAPIReady = function () {
        var list = _ytWaiting || [];
        _ytWaiting = null;
        list.forEach(function (f) { try { f(); } catch (e) { } });
    };
}

// --- đo thời lượng YouTube qua Iframe API (tạo player ẩn, đọc getDuration rồi destroy)
function probeYouTubeDuration(videoId, cb) {
    loadYTApi(function () {
        var host = document.createElement("div");
        host.id = "yt-probe-" + videoId + "-" + Date.now();
        host.style.cssText = "position:absolute;left:-9999px;width:0;height:0;overflow:hidden";
        document.body.appendChild(host);

        /* eslint-disable no-undef */
        var player = new YT.Player(host.id, {
            videoId: videoId,
            events: {
                onReady: function (ev) {
                    var secs = ev.target.getDuration ? ev.target.getDuration() : 0;
                    try { ev.target.destroy(); } catch (e) { }
                    try { document.body.removeChild(host); } catch (e) { }
                    cb(secs || 0);
                }
            }
        });
        /* eslint-enable no-undef */
    });
}

// --- cập nhật UI 1 item
function setDurationUI(spanEl, secs) {
    if (!spanEl) return;
    spanEl.textContent = secs ? fmtTime(secs) : "";
    // có thể bỏ class mờ nếu muốn nổi bật hơn:
    // spanEl.classList.remove("text-muted");
}

// --- quét sidebar, đo & cập nhật thời lượng (dùng cache nếu có)
function probeDurationsIn(container) {
    container.querySelectorAll("li.lesson").forEach(function (li) {
        var id = li.getAttribute("data-lesson-id");
        var kind = (li.getAttribute("data-kind") || '').toLowerCase();
        var durEl = li.querySelector(".js-dur");
        if (!id || !durEl) return;

        // Nếu là bài lý thuyết -> giữ nguyên nhãn và bỏ qua
        if (kind === 'text') {
            durEl.textContent = 'Lý thuyết';
            return;
        }

        if (durCache[id]) { setDurationUI(durEl, durCache[id]); return; }

        var i = idxOf(id);
        var lesson = FLAT[i];
        if (!lesson) return;

        if (lesson.kind === "video" && lesson.videoSrc) {
            probeVideoDuration(lesson.videoSrc, function (secs) {
                if (secs > 0) { durCache[id] = Math.round(secs); saveDurCache(); setDurationUI(durEl, secs); }
            });
        } else if (lesson.kind === "youtube" && lesson.youtubeId) {
            probeYouTubeDuration(lesson.youtubeId, function (secs) {
                if (secs > 0) { durCache[id] = Math.round(secs); saveDurCache(); setDurationUI(durEl, secs); }
            });
        }
    });
}



function renderSidebar(targetId) {
    var container = document.getElementById(targetId);
    if (!container) return;

    var currentQ = loadSearch();

    var html = '';
    // Ô tìm kiếm
    html += '<div class="sidebar-search">';
    html += '  <div class="input-group input-group-sm">';
    html += '    <span class="input-group-text">';
    html += '      <svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M21 21l-4.35-4.35m1.35-5.65a7 7 0 1 1-14 0 7 7 0 0 1 14 0Z" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>';
    html += '    </span>';
    html += '    <input id="lsSearch-' + targetId + '" class="form-control" style="max-width: 100vw !important" placeholder="Tìm bài học..." value="' + (currentQ || '').replace(/"/g, '&quot;') + '">';
    html += '    <button class="btn btn-clear" type="button" style="background: #ebebeb !important; border: #ebebeb" id="lsClear-' + targetId + '">&times;</button>';
    html += '  </div>';
    html += '  <small id="lsMeta-' + targetId + '"></small>';
    html += '</div>';

    // Danh sách section + lessons
    html += '<div class="sidebar-card">';
    for (var si = 0; si < COURSE_DATA.length; si++) {
        var sec = COURSE_DATA[si] || { lessons: [] };
        var learned = 0, total = sec.lessons.length;
        for (var k = 0; k < total; k++) if (state.done.indexOf(sec.lessons[k].id) >= 0) learned++;

        var secId = 'sec-' + targetId + '-' + si;
        var showClass = (window.innerWidth < 992) ? '' : 'show';

        html += '<div class="card-section" data-sec-index="' + si + '">';
        html += '  <div class="card-header d-flex justify-content-between align-items-center">';
        html += '    <button class="btn-sec-toggle collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#' + secId + '" aria-controls="' + secId + '">';
        html += '      <svg class="chev" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M5.2 2.8l5.6 5.2-5.6 5.2" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';
        html += '      <span class="sec-title">' + sec.title + '</span>';
        html += '    </button>';
        html += '    <small class="text-muted" id="sec-progress-' + targetId + '-' + si + '">(' + learned + '/' + total + ' bài đã học)</small>';
        html += '  </div>';

        html += '  <ul id="' + secId + '" class="list-group list-group-flush collapse ' + showClass + '">';
        for (var li = 0; li < sec.lessons.length; li++) {
            var l = sec.lessons[li];
            var done = state.done.indexOf(l.id) >= 0;
            var gIdx = idxOf(l.id);
            var canOpen = done || canOpenByFlatIndex(gIdx);
            var iconHtml = done ? svgCheck('text-success') : (canOpen ? svgDot() : svgLock());
            var disabledClass = canOpen ? '' : ' disabled';

            // LẤY kind từ FLAT (video | youtube | text)
            var kind = (FLAT[gIdx] && FLAT[gIdx].kind) ? FLAT[gIdx].kind : '';

            html += '<li class="list-group-item d-flex align-items-center lesson' + disabledClass + '"' +
                ' data-lesson-id="' + l.id + '"' +
                ' data-title="' + (l.title || '').replace(/"/g, '&quot;') + '"' +
                ' data-sec="' + si + '"' +
                ' data-kind="' + kind + '">';

            html += iconHtml;
            html += '<span class="flex-grow-1 lesson-title ms-2 js-title">' + l.title + '</span>';

            // Nếu là text -> hiện "Lý thuyết", còn lại để trống (sẽ được probe và lấp sau)
            var initDur = (kind === 'text') ? 'Lý thuyết' : (l.duration || '');
            html += '<small class="text-muted ms-2 js-dur">' + initDur + '</small>';

            html += '</li>';
        }

        html += '  </ul>';
        html += '</div>';
    }
    html += '</div>'; // sidebar-card

    container.innerHTML = html;

    // click lessons
    container.querySelectorAll(".lesson").forEach(function (row) {
        row.addEventListener("click", function () {
            if (this.classList.contains("disabled")) return;
            var id = this.getAttribute("data-lesson-id");
            openLessonByIndex(idxOf(id));
        });
    });

    // Search events
    var input = document.getElementById('lsSearch-' + targetId);
    var clearBtn = document.getElementById('lsClear-' + targetId);
    var meta = document.getElementById('lsMeta-' + targetId);

    var debTimer = null;
    function runFilter() {
        var q = (input.value || '').trim();
        saveSearch(q);
        applyLessonFilter(container, q, meta);
    }

    function updateClearBtn() {
        //var has = (input.value || '').trim().length > 0;
        if (clearBtn) clearBtn.classList.add('show');
    }

    input.addEventListener('input', function () {
        clearTimeout(debTimer);
        debTimer = setTimeout(runFilter, 120);
        updateClearBtn();
    });

    // Esc để xoá nhanh
    input.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && input.value) {
            input.value = '';
            runFilter();
            updateClearBtn();
        }
    });

    clearBtn.addEventListener('click', function () {
        input.value = '';
        runFilter();
        updateClearBtn();
        input.focus();
    });

    // áp dụng bộ lọc ngay (khôi phục từ localStorage)
    applyLessonFilter(container, currentQ, meta);
    updateClearBtn();
    probeDurationsIn(container);
}



function applyLessonFilter(rootEl, query, metaEl) {
    var q = (query || '').trim();
    var normQ = vnNorm(q);
    var totalShown = 0, totalAll = 0;

    // reset/ẩn hiện từng lesson
    var sections = rootEl.querySelectorAll('.card-section');
    sections.forEach(function (sec) {
        var secIdx = sec.getAttribute('data-sec-index');
        var list = sec.querySelectorAll('.lesson');
        var shownInSec = 0;
        totalAll += list.length;

        list.forEach(function (item) {
            var title = item.getAttribute('data-title') || '';
            var normTitle = vnNorm(title);
            var hit = !normQ || normTitle.indexOf(normQ) >= 0;

            // highlight lại text
            var titleSpan = item.querySelector('.js-title');
            if (titleSpan) {
                titleSpan.innerHTML = hit ? highlightHit(title, q) : title;
            }

            item.classList.toggle('d-none', !hit);
            if (hit) shownInSec++;
        });

        // Ẩn cả section nếu không có bài nào khớp
        var hasAny = shownInSec > 0;
        sec.classList.toggle('d-none', !hasAny);

        // Mở section khi có lọc & có kết quả; nếu không có q -> để nguyên
        var ul = sec.querySelector('ul');
        if (ul) {
            if (q) {
                if (shownInSec > 0 && !ul.classList.contains('show')) ul.classList.add('show');
            }
        }

        // cập nhật công tắc "(x/y bài khớp)"
        var prog = sec.querySelector('#sec-progress-' + rootEl.id + '-' + secIdx);
        if (prog) {
            var original = prog.getAttribute('data-original');
            if (!original) { prog.setAttribute('data-original', prog.textContent); original = prog.textContent; }
            if (q) prog.textContent = '(' + shownInSec + '/' + list.length + ' bài khớp)';
            else prog.textContent = original;
        }

        totalShown += shownInSec;
    });

    if (metaEl) {
        if (!q) metaEl.textContent = '';
        else metaEl.textContent = totalShown ? ('Tìm thấy ' + totalShown + ' bài học') : 'Không tìm thấy bài học';
    }
}


function redrawSidebars() {
    var cur = state.current;
    renderSidebar("sidebar");
    renderSidebar("sidebar-mobile");
    document.querySelectorAll('.lesson[data-lesson-id="' + cur + '"]').forEach(function (el) { el.classList.add('active'); });
}

// Progress + bottom bar
function updateSectionCounts() {
    for (var si = 0; si < COURSE_DATA.length; si++) {
        var ids = COURSE_DATA[si].lessons.map(function (x) { return x.id; });
        var learned = 0; for (var k = 0; k < state.done.length; k++) if (ids.indexOf(state.done[k]) >= 0) learned++;
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
function completeCurrentIfNeeded() {
    var id = state.current; if (!id) return;
    if (state.done.indexOf(id) < 0) {
        state.done.push(id); saveState();
        updateSectionCounts(); updateTotalPercent(); redrawSidebars(); syncBottomBar();
        updateMarkDoneVisibility(); // ẩn nút sau khi complete
    }
}

// Content (hiển thị phần text/HTML của bài nếu có)
function renderContentAndQuiz(lesson) {
    if (contentEl) contentEl.innerHTML = lesson.contentHtml || "";
}

// Bottom bar
var bbPrev = document.getElementById("bb-prev");
var bbNext = document.getElementById("bb-next");
var bbTitle = document.getElementById("bb-title");
function syncBottomBar() {
    var i = idxOf(state.current);
    var cur = FLAT[i] || FLAT[0];
    if (bbTitle) bbTitle.textContent = cur ? (i + 1) + ". " + cur.title : "—";
    if (bbPrev) bbPrev.disabled = i <= 0;
    if (bbNext) {
        var canNext = (i >= 0) && isUnlocked(i + 1);
        bbNext.disabled = !canNext;
    }
}
if (bbPrev) bbPrev.addEventListener("click", function () { var i = idxOf(state.current); openLessonByIndex(i - 1); });
if (bbNext) bbNext.addEventListener("click", function () { var i = idxOf(state.current); openLessonByIndex(i + 1); });
window.addEventListener("keydown", function (e) {
    if (["INPUT", "TEXTAREA"].indexOf(e.target.tagName) >= 0) return;
    var i = idxOf(state.current);
    if (e.key === "ArrowLeft") { e.preventDefault(); if (bbPrev && !bbPrev.disabled) bbPrev.click(); }
    if (e.key === "ArrowRight") { e.preventDefault(); if (bbNext && !bbNext.disabled && isUnlocked(i + 1)) bbNext.click(); }
});

// Open lesson
function openLessonByIndex(newIdx) {
    if (newIdx < 0 || newIdx >= FLAT.length) return;
    if (!isUnlocked(newIdx)) return;

    var lesson = FLAT[newIdx];
    state.current = lesson.id; saveState();

    if (lesson.kind === "youtube" && lesson.youtubeId) {
        showYouTube(lesson);
    } else if (lesson.kind === "text" && !lesson.videoSrc) {
        showText(lesson);
    } else if (lesson.videoSrc) {
        showVideo(lesson);
    } else if (lesson.kind === "text") {
        showText(lesson);
    } else {
        // fallback
        showVideo(lesson);
    }

    renderContentAndQuiz(lesson);
    updateMarkDoneVisibility(lesson); // CHỈ hiện nút cho text & youtube

    document.querySelectorAll(".lesson").forEach(function (x) { x.classList.remove("active"); });
    var active = document.querySelector('.lesson[data-lesson-id="' + lesson.id + '"]');
    if (active) active.classList.add("active");

    updateSectionCounts(); updateTotalPercent(); syncBottomBar();

    if (window.innerWidth < 992) {
        var r = document.querySelector(".ratio");
        if (r) r.scrollIntoView({ behavior: "smooth", block: "start" });
    }
}

// Init
(function init() {
    renderSidebar("sidebar");
    renderSidebar("sidebar-mobile");
    if (!state.current) state.current = CURRENT_LESSON_ID || (FLAT[0] ? FLAT[0].id : "");
    var i = idxOf(state.current); if (i < 0) i = 0;
    openLessonByIndex(i);
})();

// Toggle sidebar
(function () {
    var layout = document.getElementById('learnLayout');
    var fab = document.getElementById('btnToggleSidebar');
    var KEY = (window.COURSE_KEY || 'course:demo') + ':sidebarCollapsed';
    try { if (localStorage.getItem(KEY) === '1') layout.classList.add('is-collapsed'); } catch (e) { }
    if (fab) {
        fab.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                var el = document.getElementById('lessonDrawer');
                if (el && window.bootstrap) new bootstrap.Offcanvas(el).toggle();
                return;
            }
            layout.classList.toggle('is-collapsed');
            try { localStorage.setItem(KEY, layout.classList.contains('is-collapsed') ? '1' : '0'); } catch (e) { }
        });
    }
})();
