window.blazorKeyPressed = function(dotnetHelper) {
    document.addEventListener('keyup', function(event) {
        dotnetHelper.invokeMethodAsync('OnArrowKeyPressed', event.key);
    });
};