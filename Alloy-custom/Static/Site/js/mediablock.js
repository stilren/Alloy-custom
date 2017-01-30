var mediaBlock = function ($) {

    "use strict";

    var videosSelector = '.js-youtube-div';
    var replacementSelector = '.js-youtube-container';

    function init() {
        initListeners();
        checkThumbnail();
    }

    function initListeners() {
        $(document).on('click', videosSelector, appendVideo);
    }

    function checkThumbnail() {
        $(videosSelector)
            .each(function () {
                var getThumbnail = $(this).attr('data-get-thumbnail');
                if (getThumbnail === "True") {
                    var id = $(this).attr('data-id');
                    var replacementDiv = $(this).find(replacementSelector);
                    var image = $('<img>',
                    {
                        src: "https://i.ytimg.com/vi/" + id + "/hqdefault.jpg"
                    });

                    replacementDiv.append(image);
                }
            });
    }

    function appendVideo() {
        var url = $(this).attr('data-id');
        var replacementDiv = $(this).find(replacementSelector);

        var iframe = $('<iframe>',
        {
            src: url + '?autoplay=1',
            frameborder: 0,
            scrolling: 'no',
            allowfullscreen: 1,
        });

        replacementDiv.replaceWith(iframe);
    }

    // 'Public'
    return {
        init: init
    }

}(jQuery).init();