
function showHint(data) {
    jQuery("#hint").children(".btn").remove();
    var el = jQuery("<span>Nápoveda:<b> " + data.Hint + "</b></span>")
    el.hide();
    jQuery("#hint").prepend(el);
    el.fadeIn(250);
}

