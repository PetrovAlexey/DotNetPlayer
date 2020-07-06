// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//like & shuffle button
$('.heart').click(function () {
    $(this).toggleClass('clicked');
});

$('.shuffle').click(function () {
    $(this).toggleClass('clicked');
});

//show info box on hover
$('#player').hover(function () {
    $('.info').toggleClass('up');
});

//music player settings

let audio = new Audio("/Files/audio.mp3");

$('.pause').hide(); //hide pause button until clicked

//play button
$('.play').click(function () {
    audio.play();
    $('.play').hide();
    $('.pause').show();
});

//pause button
$('.pause').click(function () {
    audio.pause();
    $('.play').show();
    $('.pause').hide();
});
