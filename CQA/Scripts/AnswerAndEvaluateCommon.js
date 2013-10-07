//Todo add to standalone js file because it is the same for evaluating
function showHint(data) {
    jQuery("#hint").children(".btn").remove();
    var el = jQuery("<span>Nápoveda:<b> " + data.Hint + "</b></span>")
    el.hide();
    jQuery("#hint").prepend(el);
    el.fadeIn(250);
}

