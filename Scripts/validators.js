//==============================================================================================
function validateString(str) {
    if (str == null)
        return false;
    if (str.length <= 0)
        return false;

    return true;
}
//==============================================================================================
function validateNumber(num) {
    var number;
    
    if (!validateString(num))
        return false;

    if (isNaN(num))
        return false;
    
    return true;
}
//==============================================================================================
function validateInputNumberRunTime(event, el) {
    return validateInputNumber(event, el, false);
}
//==============================================================================================
function validateInputPriceRunTime(event, el) {
    return validateInputNumber(event, el, true);
}
//==============================================================================================
function validateInputNumber(event, el, allowDecimal)
{
    var keyPressed, keyChar;

    keyPressed = getKeyPressed(event);

    if (keyPressed == null) return true;
    keyChar = String.fromCharCode(keyPressed);

    // check arrows and system keys
    if (keyPressed == 27 || keyPressed == 13 || keyPressed == 9 || keyPressed == 8 || keyPressed == 0)
        return true;

    // check numbers
    if ((("0123456789").indexOf(keyChar) > -1))
        return true;

    // check if allowed decimal point and check if exist one
    if (allowDecimal && keyChar == "." && el.value.indexOf(".") <= 0)
        return true;
    
    return false;
}
//==============================================================================================
function getKeyPressed(event) {
    if (window.event) {
        return window.event.keyCode;
    }
    else {
        if (event) {
            return event.which;
        }
        else {
            return null;
        }
    }
}
//==============================================================================================