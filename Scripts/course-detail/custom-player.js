(function(){
    var container = document.getElementById('player');
    var video     = document.getElementById('lessonVideo');
    var playBtn   = document.getElementById('vc-play');
    var icoPlay   = document.getElementById('ico-play');
    var icoPause  = document.getElementById('ico-pause');

    var progWrap  = document.getElementById('vc-progress');
    var progBar   = document.getElementById('vc-bar');
    var curEl     = document.getElementById('vc-cur');
    var durEl     = document.getElementById('vc-dur');

    var muteBtn   = document.getElementById('vc-mute');
    var icoVol    = document.getElementById('ico-vol');
    var icoMute   = document.getElementById('ico-mute');
    var volRange  = document.getElementById('vc-volume');
    var speedSel  = document.getElementById('vc-speed');
    var fullBtn   = document.getElementById('vc-full');

    function fmt(t){
    if (!isFinite(t)) return "00:00";
    var h = Math.floor(t/3600);
    var m = Math.floor((t%3600)/60);
    var s = Math.floor(t%60);
    return (h>0? String(h).padStart(2,'0')+':' : '') +
    String(m).padStart(2,'0')+':'+ String(s).padStart(2,'0');
  }

    function updatePlayIcon(){
    var playing = !video.paused && !video.ended;
    icoPlay.style.display  = playing ? 'none' : '';
    icoPause.style.display = playing ? '' : 'none';
  }

    function updateVolIcon(){
    var muted = video.muted || video.volume === 0;
    icoVol.style.display  = muted ? 'none' : '';
    icoMute.style.display = muted ? '' : 'none';
  }

    function updateProgress(){
    var pct = (video.currentTime / (video.duration || 1)) * 100;
    progBar.style.insetInlineEnd = (100 - pct) + '%'; // kiểm soát width
    progWrap.setAttribute('aria-valuenow', Math.round(pct));
    curEl.textContent = fmt(video.currentTime);
  }

    // events
    playBtn.addEventListener('click', function(){
    if (video.paused) video.play(); else video.pause();
  });
    video.addEventListener('play',  updatePlayIcon);
    video.addEventListener('pause', updatePlayIcon);
    video.addEventListener('ended', updatePlayIcon);

    video.addEventListener('loadedmetadata', function(){
        durEl.textContent = fmt(video.duration);
    updateProgress();
  });
    video.addEventListener('timeupdate', updateProgress);

    // seek
    function seekAtClientX(x){
    var rect = progWrap.getBoundingClientRect();
    var pct = Math.min(1, Math.max(0, (x - rect.left)/rect.width));
    video.currentTime = pct * (video.duration || 0);
  }
    progWrap.addEventListener('click', function(e){seekAtClientX(e.clientX); });
    // kéo rê
    var dragging=false;
    progWrap.addEventListener('pointerdown', function(e){dragging = true; seekAtClientX(e.clientX); progWrap.setPointerCapture(e.pointerId); });
    progWrap.addEventListener('pointermove', function(e){ if(dragging) seekAtClientX(e.clientX); });
    progWrap.addEventListener('pointerup',   function(e){dragging = false; progWrap.releasePointerCapture(e.pointerId); });

    // volume
    volRange.addEventListener('input', function(){
        video.volume = parseFloat(this.value);
    if (video.volume > 0) video.muted = false;
    updateVolIcon();
  });
    muteBtn.addEventListener('click', function(){
        video.muted = !video.muted;
    if (!video.muted && video.volume === 0) {video.volume = 0.5; volRange.value = 0.5; }
    updateVolIcon();
  });

    // speed
    speedSel.addEventListener('change', function(){
        video.playbackRate = parseFloat(this.value);
  });

    // fullscreen
    fullBtn.addEventListener('click', function(){
    var el = container;
    if (document.fullscreenElement) document.exitFullscreen();
    else if (el.requestFullscreen) el.requestFullscreen();
  });

    // keyboard
    container.addEventListener('keydown', function(e){
    if (e.target && ['INPUT','SELECT'].includes(e.target.tagName)) return;
    if (e.code === 'Space') {e.preventDefault(); playBtn.click(); }
    if (e.code === 'ArrowLeft')  {video.currentTime = Math.max(0, video.currentTime - 5); }
    if (e.code === 'ArrowRight') {video.currentTime = Math.min(video.duration || 0, video.currentTime + 5); }
    if (e.key === 'm' || e.key === 'M') muteBtn.click();
    if (e.key === 'f' || e.key === 'F') fullBtn.click();
  });

    // auto-hide controls khi đứng yên chuột
    var hideTimer;
    function showControls(){
        container.classList.remove('hide-controls');
    clearTimeout(hideTimer);
    hideTimer = setTimeout(function(){container.classList.add('hide-controls'); }, 2000);
  }
    container.addEventListener('mousemove', showControls);
    container.addEventListener('touchstart', showControls, {passive:true});
    showControls(); // khởi động

    // cập nhật icon âm lượng ban đầu
    updateVolIcon();
    updatePlayIcon();
})();
