//Todo add to standalone js file because it is the same for evaluating
function showHint(data) {
    $("#hint").children(".btn").remove();
    var el = $("<span>Nápoveda:<b> "+data.Hint+"</b></span>")
    el.hide();
    $("#hint").prepend(el);
    el.fadeIn(250);
}

