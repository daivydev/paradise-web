// ======= course-detail.js (stable video switch: video | youtube | text) =======

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

function renderSidebar(targetId) {
    var container = document.getElementById(targetId);
    if (!container) return;

    var html = '<div class="sidebar-card">';
    for (var si = 0; si < COURSE_DATA.length; si++) {
        var sec = COURSE_DATA[si] || { lessons: [] };
        var learned = 0;
        for (var k = 0; k < sec.lessons.length; k++) if (state.done.indexOf(sec.lessons[k].id) >= 0) learned++;
        var secId = 'sec-' + targetId + '-' + si;
        var showClass = (window.innerWidth < 992) ? '' : 'show';

        html += '<div class="card-section">';
        html += '<div class="card-header d-flex justify-content-between align-items-center">';
        html += '<button class="btn-sec-toggle collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#' + secId + '" aria-controls="' + secId + '">';
        html += '<svg class="chev" width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M5.2 2.8l5.6 5.2-5.6 5.2" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';
        html += '<span>' + sec.title + '</span></button>';
        html += '<small class="text-muted" id="sec-progress-' + si + '">(' + learned + '/' + sec.lessons.length + ' bài đã học)</small></div>';

        html += '<ul id="' + secId + '" class="list-group list-group-flush collapse ' + showClass + '">';
        for (var li = 0; li < sec.lessons.length; li++) {
            var l = sec.lessons[li];
            var done = state.done.indexOf(l.id) >= 0;
            var gIdx = idxOf(l.id);
            var canOpen = done || canOpenByFlatIndex(gIdx);
            var iconHtml = done ? svgCheck('text-success') : (canOpen ? svgDot() : svgLock());
            var disabledClass = canOpen ? '' : ' disabled';
            html += '<li class="list-group-item d-flex align-items-center lesson' + disabledClass + '" data-lesson-id="' + l.id + '">' + iconHtml +
                '<span class="flex-grow-1 lesson-title ms-2">' + l.title + '</span>' +
                '<small class="text-muted ms-2">' + (l.duration || '') + '</small></li>';
        }
        html += '</ul></div>';
    }
    html += '</div>';
    container.innerHTML = html;

    container.querySelectorAll(".lesson").forEach(function (row) {
        row.addEventListener("click", function () {
            if (this.classList.contains("disabled")) return;
            var id = this.getAttribute("data-lesson-id");
            openLessonByIndex(idxOf(id));
        });
    });
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
