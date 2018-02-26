// Write your Javascript code.
var highlightFields = function (response) {
    $('.form-control').removeClass('is-invalid');

    $.each(response, function (propName, val) {
        var nameSelector = '[name = "' + propName.replace(/(:|\.|\[|\])/g, "\\$1") + '"]',
            idSelector = '#' + propName.replace(/(:|\.|\[|\])/g, "\\$1");
        var $el = $(nameSelector) || $(idSelector);

        if (val.Errors.length > 0) {
            $el.addClass('is-invalid');
        }
    });
};

var highlightErrors = function (xhr) {
    var data = JSON.parse(xhr.responseText);
    highlightFields(data);
    showSummary(data);
    window.scrollTo(0, 0);
};

var showSummary = function (response) {
    $('#validationSummary').empty().removeClass('d-none');

    var verboseErrors = _.flatten(_.map(response, 'Errors')),
        errors = [];

    var nonNullErrors = _.reject(verboseErrors, function (error) {
        return error.ErrorMessage.indexOf('must not be empty') > -1;
    });

    _.each(nonNullErrors, function (error) {
        errors.push(error.ErrorMessage);
    });

    if (nonNullErrors.length !== verboseErrors.length) {
        errors.push('The highlighted fields are required to submit this form.');
    }

    var html = '<ul>';

    _.each(errors, function (error) {
        html += '<li>' + error + '</li>';
    });

    html += '</ul>';
    $('#validationSummary').append(html);
};

var redirect = function (data) {
    if (data.redirect) {
        window.location = data.redirect;
    } else {
        window.scrollTo(0, 0);
        window.location.reload();
    }
};

var handleServerError = function (xhr) {
    $('#validationSummary').empty().removeClass('d-none');

    var data = JSON.parse(xhr.responseText);
    var html = '<ul><li>' + data.message + '</li><ul>';

    $('#validationSummary').append(html);
};

$('form[method=post]').not('.no-ajax').on('submit', function () {
    var submitBtn = $(this).find('[type="submit"]');

    submitBtn.prop('disabled', true);
    $(window).unbind();

    var haveFiles = $(this).find('[type="file"]').length > 0 ? true : false;
    var $this = $(this);
    var formData = haveFiles ? new FormData($(this)[0]) : $this.serialize();
    var contentType = haveFiles ? false : 'application/x-www-form-urlencoded; charset=UTF-8';
    var processData = haveFiles ? false : true;

    $this.find('div').removeClass('is-invalid');

    $.ajax({
        url: $this.attr('action'),
        type: 'post',
        data: formData,
        contentType: contentType,
        processData: processData,
        dataType: 'json',
        statusCode: {
            200: redirect,
            400: function (xhr) {
                highlightErrors(xhr);
            },
            500: function (xhr) {
                handleServerError(xhr);
            }
        },
        complete: function () {
            submitBtn.prop('disabled', false);
        }
    });

    return false;
});


var headers = $('#accordions .accordion-header');
//var headersTwo = $('#accordions .accordion-header');
var contentAreas = $('#accordions .ui-accordion-content').hide();
// var contentAreasTwo = $('#accordion .ui-accordion-content').hide();
var expandLink = $('.accordion-expand-all');
var expandClicked = false;
// add the accordion functionality
headers.on('click', function () {
    var panel = $(this).next();
    var isOpen = panel.is(':visible');

    // open or close as necessary
    panel[isOpen ? 'slideUp' : 'slideDown']();
    // trigger the correct custom event
    // isOpen ? panel.slideUp() : panel.slideDown();
    //.trigger(isOpen ? 'hide' : 'show');
    // alert('open');

    // stop the link from causing a pagescroll
    return false;
});


// hook up the expand/collapse all
expandLink.click(function () {
    var isAllOpen = $(this).data('isAllOpen');
    expandClicked = true;
    //contentAreas[isAllOpen ? 'hide' : 'show']()
    //    .trigger(isAllOpen ? 'hide' : 'show');
    contentAreas['hide']()
        .trigger('hide');

    //contentAreasTwo[isAllOpen ? 'hide' : 'show']()
    //  .trigger(isAllOpen ? 'hide' : 'show');
    contentAreasTwo['hide']()
        .trigger('hide');

    var listItems = $("#locAccordions li");

    listItems.each(function (idx, li) {
        $('#' + $(li).text()).parent().removeClass('is-expanded').addClass('is-collapsed');
    });

});

setAriaAttr = function (el, ariaType, newProperty) {
    el.setAttribute(ariaType, newProperty);
};
setAccordionAria = function (el1, el2, expanded) {
    switch (expanded) {
        case "true":
            setAriaAttr(el1, 'aria-expanded', 'true');
            setAriaAttr(el2, 'aria-hidden', 'false');
            break;
        case "false":
            setAriaAttr(el1, 'aria-expanded', 'false');
            setAriaAttr(el2, 'aria-hidden', 'true');
            break;
        default:
            break;
    }
};
//f
// when panels open or close, check to see if they're all open
contentAreas.on({
    // whenever we open a panel, check to see if they're all open
    // if all open, swap the button to collapser
    show: function () {
        var isAllOpen = !contentAreas.is(':hidden');
        if (isAllOpen || expandClicked) {
            expandLink.text('Collapse All')
                .data('isAllOpen', true);

        }
    },
    // whenever we close a panel, check to see if they're all open
    // if not all open, swap the button to expander
    hide: function () {
        var isAllOpen = !contentAreas.is(':hidden');
        expandClicked = false;
        if (!isAllOpen) {
            //expandLink.text('Expand all')
            expandLink.text('Collapse All')
                .data('isAllOpen', false);
        }
    }
});


(function () {
    var d = document,
        accordionToggles = d.querySelectorAll('.js-accordionTrigger'),
        setAria,
        setAccordionAria,
        switchAccordion,
        touchSupported = ('ontouchstart' in window),
        pointerSupported = ('pointerdown' in window);

    skipClickDelay = function (e) {
        e.preventDefault();
        e.target.click();
    }

    setAriaAttr = function (el, ariaType, newProperty) {
        el.setAttribute(ariaType, newProperty);
    };
    setAccordionAria = function (el1, el2, expanded) {
        switch (expanded) {
            case "true":
                setAriaAttr(el1, 'aria-expanded', 'true');
                setAriaAttr(el2, 'aria-hidden', 'false');
                break;
            case "false":
                setAriaAttr(el1, 'aria-expanded', 'false');
                setAriaAttr(el2, 'aria-hidden', 'true');
                break;
            default:
                break;
        }
    };
    //function
    switchAccordion = function (e) {

        e.preventDefault();
        //alert(e.target.nextElementSibling);
        //alert(e.target.parentNode.parentNode.nextElementSibling);
        //alert(e.target);
        //  var thisAnswer = e.target.parentNode.nextElementSibling;
        var thisAnswer = e.target.nextElementSibling != null ? e.target.nextElementSibling : e.target.parentNode.parentNode.nextElementSibling;
        var thisQuestion = e.target.nextElementSibling != null ? e.target : e.target.parentNode.parentNode;
        var target = e.target;

        if (thisAnswer.classList.contains('is-collapsed')) {
            setAccordionAria(thisQuestion, thisAnswer, 'true');
        } else {
            setAccordionAria(thisQuestion, thisAnswer, 'false');
        }
        thisQuestion.classList.toggle('is-collapsed');
        thisQuestion.classList.toggle('is-expanded');
        thisAnswer.classList.toggle('is-collapsed');
        thisAnswer.classList.toggle('is-expanded');

        thisAnswer.classList.toggle('animateIn');
    };
    for (var i = 0, len = accordionToggles.length; i < len; i++) {
        if (touchSupported) {
            accordionToggles[i].addEventListener('touchstart', skipClickDelay, false);
        }
        if (pointerSupported) {
            accordionToggles[i].addEventListener('pointerdown', skipClickDelay, false);
        }
        accordionToggles[i].addEventListener('click', switchAccordion, false);
    }
})();

//uses classList, setAttribute, and querySelectorAll
//if you want this to work in IE8/9 youll need to polyfill these
(function () {
    var d = document,
        accordionToggles = d.querySelectorAll('.js-accordionTrigger'),
        setAria,
        setAccordionAria,
        switchAccordion,
        touchSupported = ('ontouchstart' in window),
        pointerSupported = ('pointerdown' in window);

    skipClickDelay = function (e) {
        e.preventDefault();
        e.target.click();
    }

    setAriaAttr = function (el, ariaType, newProperty) {
        el.setAttribute(ariaType, newProperty);
    };
    setAccordionAria = function (el1, el2, expanded) {
        switch (expanded) {
            case "true":
                setAriaAttr(el1, 'aria-expanded', 'true');
                setAriaAttr(el2, 'aria-hidden', 'false');
                break;
            case "false":
                setAriaAttr(el1, 'aria-expanded', 'false');
                setAriaAttr(el2, 'aria-hidden', 'true');
                break;
            default:
                break;
        }
    };
    //function
    switchAccordion = function (e) {
        e.preventDefault();
        var thisAnswer = e.target.parentNode.nextElementSibling;
        var thisQuestion = e.target;
        if (thisAnswer.classList.contains('is-collapsed')) {
            setAccordionAria(thisQuestion, thisAnswer, 'true');
        } else {
            setAccordionAria(thisQuestion, thisAnswer, 'false');
        }
        thisQuestion.classList.toggle('is-collapsed');
        thisQuestion.classList.toggle('is-expanded');
        thisAnswer.classList.toggle('is-collapsed');
        thisAnswer.classList.toggle('is-expanded');

        thisAnswer.classList.toggle('animateIn');
    };
    for (var i = 0, len = accordionToggles.length; i < len; i++) {
        if (touchSupported) {
            accordionToggles[i].addEventListener('touchstart', skipClickDelay, false);
        }
        if (pointerSupported) {
            accordionToggles[i].addEventListener('pointerdown', skipClickDelay, false);
        }
        accordionToggles[i].addEventListener('click', switchAccordion, false);
    }
})();