
"use strict";

var dalwizey = (function () {
    var cvPage = "#coverpage";
    var titleStore = "#titleStore";
    var pagecontainer = "#page-container";
    var slideUpCover = function () {
        $(cvPage).slideUp();
        var ht = $(document).width();
        if (ht < 768) {
            var mnu = $(".navbar-collapse");
            mnu.removeClass("in");
            mnu.attr("aria-expanded", false);
            mnu.css("height", "1px");
        }
    };
    var setActive = function () {
        var clickedElement = $(this);
        var activeElement = clickedElement.siblings("a.getr.active").first();
        if (activeElement.length !== 0) activeElement.removeClass("active");
        clickedElement.addClass("active");
    };
    var updateUI = function (data, pageUrl, phost, IsPop) {
        var elem = this;
        if (!phost) phost = pagecontainer;
        $(phost).html(data);
        slideUpCover();
        dalwizey.UploadDoc();
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
        slideUpCover();
        dalwizey.UploadDoc();
    };
    return {
        coverPage: $(cvPage),
        ToastTypes: {
            info: "info",
            success: "success",
            danger: "danger"
        },
        Toast: function(toast){
            var mToast = $(toast);
            mToast.insertBefore("#coverpage");
            mToast.show("slow");
        },
        getPage: function (IsPop) {
            var elem = this;
            var pageUrl = $(this).attr("href");
            var phost = $(this).attr("data-host");
            if (!pageUrl || $(this).hasClass("chatconnector")) pageUrl = $(this).attr("data-url");
            if (pageUrl.indexOf("#") === -1) {
                if (pageUrl.indexOf("?") === -1) pageUrl += "?r=1"; else pageUrl += "&r=1";
                return $.ajax({
                    url: pageUrl,
                    method: "GET"
                }).fail(function (data) {
                    slideUpCover();
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
                slideUpCover();
                alert(data.statusText);
            }).done(function (data) {
                updatePanel(data, phost)
            });
        },
        payStack: function (_key, _email, _amount, _ref) {
            PaystackPop.setup({
                key: _key,
                email: _email,
                amount: _amount,
                container: 'paystackHost',
                callback: function (resp) {
                    alert('successfully subscribed. transaction ref is ' + response.reference);
                },
            });
        },
        UploadDoc: function (targetUrl, targetParam, acceptedfileTypes) {
            var upldr = document.getElementById("fileUploader");
            if (upldr) {
                if (!targetParam) targetParam = "file";
                if (!targetUrl) targetUrl = "/account/UpdateAvatar";
                if (!acceptedfileTypes) acceptedfileTypes = "image/*";
                $("#fileUploader:not(.dz-clickable)").dropzone({
                    url: targetUrl,
                    paramName: targetParam, // The name that will be used to transfer the file
                    maxFilesize: 2, // MB
                    acceptedFiles: acceptedfileTypes,
                    previewsContainer: "#docPreview",
                    init: function () {
                        var dz = this;
                        dz.on("sending", function (file, xhr, formData) {
                            dalwizey.coverPage.show();
                            var rvt = document.getElementsByName("__RequestVerificationToken")[0];
                            if (rvt) formData.append("__RequestVerificationToken", $(rvt).val());
                        });
                        dz.on("success", function (file, resp) {
                            //$(upldr).attr("src", "data:" + file.type + ";base64," + resp);
                            dalwizey.Toast(resp);
                        });
                        dz.on("error", function (file, msg, xhr) {
                            alert(msg);
                        });
                        dz.on("complete", function (file) {
                            slideUpCover();
                            dz.removeFile(file);
                        });
                    }
                });
            }
        }
    }
})()

$(function () {

    //$.ajaxSetup({
    //    beforesend: function (request) {
    //        request.setRequestHeader("X-Requested-With", "XMLHttpRequest");
    //    }
    //})

    $(".hider").on("click", function () {
        var tgt = $(this).attr("data-target");
        $(tgt).hide(300, function () {
            $(tgt + " > #imgPreview").html();
        });
    })

    $(window).on("popstate", function () {
        var st = event.state;
        if (!st) return false;
        var elem = document.querySelector("a.getr[href='" + st.pageUrl + "']");
        if (!elem) return false;
        dalwizey.getPage.call(elem, true);
    })

    $(document).on("click", ".getr", function (e) {
        e.preventDefault();
        dalwizey.coverPage.show();
        dalwizey.getPage.call(this, false);
    });
    
    $(document).on("click", ".postr", function (e) {
        e.preventDefault();
        dalwizey.coverPage.show();
        dalwizey.postData.call(this, false);
    });

    $(document).on("click", ".payr", function (e) {
        var frma = document.getElementById("frmActivate");
        if(frma !== null) $(frma).append('<input type="hidden" name="r" value="1" />');
    });

    $(document).on("change", "#Gender", function (e) {
        var g = $(this).val();
        var avt = $("#Avatar");
        avt.hide("fast");
        avt.attr("src", "/images/" + g + "Avatar.png");
        avt.show("fast");
    })

    Dropzone.autoDiscover = false;
    dalwizey.UploadDoc();
    
    //$("#adsPanel").resizeable();
})
