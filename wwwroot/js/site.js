(function ($) {
  'use strict';

  //   navfixed
  $(window).on('scroll', function () {
    var scrolling = $(this).scrollTop();

    if (scrolling > 10) {
      $('.naviagtion').addClass('nav-bg');
    } else {
      $('.naviagtion').removeClass('nav-bg');
    }
  });

  // Background-images
  $('[data-background]').each(function () {
    $(this).css({
      'background-image': 'url(' + $(this).data('background') + ')'
    });
  });

  // venobox popup 
  $('.venobox').venobox();

  //  Count Up
  function counter() {
    var oTop;
    if ($('.counter').length !== 0) {
      oTop = $('.counter').offset().top - window.innerHeight;
    }
    if ($(window).scrollTop() > oTop) {
      $('.counter').each(function () {
        var $this = $(this),
          countTo = $this.attr('data-count');
        $({
          countNum: $this.text()
        }).animate({
          countNum: countTo
        }, {
          duration: 2000,
          easing: 'swing',
          step: function () {
            $this.text(Math.floor(this.countNum));
          },
          complete: function () {
            $this.text(this.countNum);
          }
        });
      });
    }
  }
  $(window).on('scroll', function () {
    counter();
  });

  // Aos js
  AOS.init({
    once: true,
    offset: 250,
    easing: 'ease',
    duration: 800
  });

  $("#form_register").on("submit", function (event) {
    event.preventDefault();
    $("form_register_error").hide();

    // Validate username and password
    var errors = [];
    const good = new RegExp(/^\w{4,20}$/i);
    if (!good.test(event.target[0].value)) {
      errors.push("Pick a better username");
    }
    if (!good.test(event.target[1].value)) {
      errors.push("Pick a stronger password");
    }

    // Verify invitation code on client side to prevent brute force load on server
    if (btoa(event.target[2].value) != "ZmxhZ3tub19zM2NyZTdzXzFuX0o0dmE1Y3IxcHR9") {
      errors.push("Invalid invitation code");
    }

    if (errors.length) {
      $("#form_register_error").html(errors.join("<br />")).show().fadeOut(5000);
    } else {
      $.ajax({
        url: event.currentTarget.action,
        type: 'post',
        data: $("#form_register").serialize(),
        success: function (data, textStatus, jqXHR) {
          window.location.href = window.location.href.split('/')[0];
        },
        error: function (xhr) {
          $("#form_register_error").html(xhr.responseText).show().fadeOut(5000);
        }
      });
    }
  });

  $("form[name='noteUpdate']").on("submit", function (event) {
    event.preventDefault();
    $.ajax({
      url: event.currentTarget.action,
      type: 'post',
      data: $(this).serialize(),
      success: function (data, textStatus, jqXHR) {
        window.location.href = window.location.href.split('/')[0];
      },
      error: function (xhr) {
        console.log(xhr.responseText);
      }
    });
  });

})(jQuery);