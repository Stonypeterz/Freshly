/*
Dalwiz Default Template Javascript. // Do not delete this line while editting this script if you'll still regenerate your code.
*/
var dalWiz = (function () {
    var titleStore = "#titleStore";
    var pagecontainer = "#page-container";
    var setActive = function () {
        var clickedElement = $(this);
        var activeElement = clickedElement.siblings("a.getr.active").first();
        activeElement.removeClass("active");
        clickedElement.addClass("active");
    };
    var updateUI = function (data, pageUrl, phost, IsPop) {
        var elem = this;
        if (!phost) phost = pagecontainer;
        $(phost).html(data);
        if (IsPop === false && pageUrl !== window.location) {
            var pageTitle = $(titleStore).val();
            document.title = pageTitle;
            var modUrl = pageUrl.replace("?r=1", "").replace("&r=1", "");
            window.history.pushState({ pageUrl: modUrl }, pageTitle, modUrl);
        }
        setActive.call(elem);
    };
    var updatePanel = function (data, phost) {
        if (!phost) phost = pagecontainer;
        $(phost).html(data);
    };
    return {
        getPage: function (IsPop) {
            var elem = this;
            var pageUrl = $(this).attr("href");
            var phost = $(this).attr("data-host");
            if (!pageUrl) pageUrl = $(this).attr("data-url");
            if (pageUrl.indexOf("#") === -1) {
                if (pageUrl.indexOf("?") === -1) pageUrl += "?r=1"; else pageUrl += "&r=1";
                return $.ajax({
                    url: pageUrl,
                    method: "GET"
                }).fail(function (data) {
                    alert(data.statusText);
                }).done(function (data) {
                    updateUI.call(elem, data, pageUrl, phost, IsPop)
                });
            }
        },
        postData: function (IsPop) {
            var $elem = $(this);
            var frm = $elem.closest("form");
            var params = frm.serialize();
            var pageUrl = frm.attr("action");
            var phost = $elem.attr("data-host");
            if (!pageUrl) pageUrl = $elem.attr("data-url");
            if (pageUrl.indexOf("?") === -1) pageUrl += "?r=1"; else pageUrl += "&r=1";
            return $.ajax({
                url: pageUrl,
                data: params,
                method: "POST"
            }).fail(function (data) {
                alert(data.statusText);
            }).done(function (data) {
                updatePanel(data, phost)
            });
        }
    }
})()

$(function () {
    
    $.ajaxSetup({
        beforesend: function (request) {
            request.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        }
    })

    $(window).on("popstate", function () {
        var st = event.state;
        if (!st) return false;
        var elem = document.querySelector("a.getr[href='" + st.pageUrl + "']");
        if (!elem) return false;
        dalWiz.getPage.call(elem, true);
    })

    $(document).on("click", ".getr", function (e) {
        e.preventDefault();
        dalWiz.getPage.call(this, false);
    });

    $(document).on("click", ".postr", function (e) {
        e.preventDefault();
        dalWiz.postData.call(this, false);
    });

    //$("#adsPanel").resizeable();
})
