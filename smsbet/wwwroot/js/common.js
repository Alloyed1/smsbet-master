$(document).ready(function() {

	//табы дисциплин
	$('ul.list_disciplines a').click(function(e) {
        e.preventDefault();
        $('ul.list_disciplines .active').removeClass('active');
        $(this).addClass('active');
        var tab = $(this).attr('href');
        $('.tab_disciplines_content').not(tab).css({'display':'none'});
        $(tab).fadeIn(400);
    });

	//кнопка скрыть инфо
    $('.close_info_window').on('click', function(e) {
    	e.preventDefault();
    	$('.info_window_block').slideUp(400);

    });

    


    //открытие скрытых блоков
    $('.all_developments a').on('click', function(e) {
        e.preventDefault();
        if ($(this).hasClass('active')) {
            $('.btn_all_develop span').text("Еще события");
            $(this).removeClass('active');
        }
        else {
            $('.btn_all_develop span').text("Свернуть");
            $(this).addClass('active');
        }

       $('.open_block').slideToggle(500);
  });


    // меню
    $('.menuToggle').on('click', function() {
        $(this).addClass('active');
        $('.main_nav_toggle').slideToggle(400, function(){
            if( $(this).css('display') === "none"){
                $(this).removeAttr('style');
                $('.menuToggle').removeClass('active');
            }
            // if( $(this).css('display') === "block"){
            //     $(this).css('display', 'flex');
            // }
        });
    });



	//Попап менеджер FancyBox
	// data-fancybox="gallery" создание галереи
	// data-caption="<b>Подпись</b><br>"  Подпись картинки
	// data-width="2048" реальная ширина изображения
	// data-height="1365" реальная высота изображения
	// data-type="ajax" загрузка контента через ajax без перезагрузки
	// data-type="iframe" загрузка iframe (содержимое с другого сайта)
	$(".fancybox").fancybox({
		hideOnContentClick: true,
		protect: false, //защита изображения от загрузки, щелкнув правой кнопкой мыши.
		loop: true, // Бесконечная навигация по галерее
		arrows : true, // Отображение навигационные стрелки
		infobar : true, // Отображение инфобара (счетчик и стрелки вверху)
		toolbar : true, // Отображение панели инструментов (кнопки вверху)
		buttons : [ // Отображение панели инструментов по отдельности (кнопки вверху)
        // 'slideShow',
        // 'fullScreen',
        // 'thumbs',
        // 'share',
        //'download',
        //'zoom',
        'close'
    	],
    	touch: false,
    	animationEffect : "zoom-in-out", // анимация открытия слайдов "zoom" "fade" "zoom-in-out"
    	transitionEffect: 'slide', // анимация переключения слайдов "fade" "slide" "circular" "tube" "zoom-in-out" "rotate'
    	animationDuration : 500, // Длительность в мс для анимации открытия / закрытия
    	transitionDuration : 1366, // Длительность переключения слайдов
    	slideClass : '', // Добавить свой класс слайдам

	});

	// Маска для формы телефона https://github.com/RobinHerbots/Inputmask

    $("input[type='tel']").inputmask({
	  mask: '+9 (999) 999 99-99',
	  showMaskOnHover: false,
	  autoUnmask: true,
    });

    

    if ($(".slider_add_event").length > 0) {
        var swiper = new Swiper('.slider_add_event', {
            // direction: 'vertical', // вертикальный слайдер
            slidesPerView: 4,
            spaceBetween: 5,
            // effect: 'fade', // анимация
            loop: false,
            centeredSlides: false,
            observer: false, // помощь инициализации
            observeParents: false,
            slidesPerGroup: 1,
            slideToClickedSlide: false, // клик на слайд = переход на слайд
            watchOverflow: true, // уберет навигацию когда она не нужна
            // autoplay: {
         //        delay: 2500,
         //        disableOnInteraction: false,
      //        },
            navigation: {
                nextEl: '.slider_add_event_knob_next',
                prevEl: '.slider_add_event_knob_prev',
            },
            breakpoints: {
                1200: {
                  slidesPerView: 4,
                },
                768: {
                  slidesPerView: 3,
                },
                640: {
                  slidesPerView: 1,
                }
            }
        });
    }



});

let phoneNumber;

//восстановление пароля
$('#sendsms_btn').on('click', function () {

    $('#sendsms_btn').attr('hidden', true);
    $('#sendsms_btn_spinner').attr('hidden', false);
    $('#sendsms_btn_spinner').prop('disabled', true);

    $.ajax({
        type: 'GET',
        url: '/Account/ResendPassword',
        data: {
            phone: $('#reset_pass_input').val()
        },
        success: function (res) {
            if (res == "ok") {
                $('#incorrect_data').attr('hidden', true);
                $('#account_btn').click();
            }
            else if (res == "badsms") {
                $('#sms_error_res').attr('hidden', false);
                $('#sendsms_btn').attr('hidden', false);
                $('#sendsms_btn_spinner').prop('disabled', false);
                $('#sendsms_btn_spinner').attr('hidden', true);
                
            }
            else if (res == "bad") {
                $('#incorrect_num_res').attr('hidden', false);
                $('#sendsms_btn').attr('hidden', false);
                $('#sendsms_btn_spinner').prop('disabled', false);
                $('#sendsms_btn_spinner').attr('hidden', true);
            }
        }

    });
});


//авторизация
$('#signin_btn').on('click', function () {
    $('#signin_btn').attr('hidden', true);
    $('#signin_btn_spinner').attr('hidden', false);
    $('#ssignin_btn_spinner').prop('disabled', true);
    $('#incorrect_data').attr('hidden', true);

   

    $.ajax({
        type: 'POST',
        url: '/Account/Auth',
        data: {
            phone: $('#phone_input_log').val(),
            pass: $('#pass_input_log').val()
        },
        success: function (res) {
            if (res == true) {
                $.cookie("phone", $('#phone_input_log').val());
                location.reload();
            }
            else {
                $('#signin_btn').attr('hidden', false);
                $('#signin_btn_spinner').attr('hidden', true);
                $('#ssignin_btn_spinner').prop('disabled', false);
                $('#incorrect_data').attr('hidden', false);
            }
        }
    });

});


//проверка пароля
$("#confirm_password").on('click', function () {
    $("#confirm_password").attr("hidden", true);
    $("#confirm_password_btn").attr("hidden", false);
    $('#confirm_password_btn').prop('disabled', false);
    $.ajax({
        type: 'GET',
        url: '/Account/CheckPassword',
        data: {
            password: $('#pass_input').val(),
            phone: phoneNumber
        },
        success: function (res) {
            console.log(res);
            if (res == true) {
                console.log('успешно');
                location.reload();
            }
            else {
                $("#incorrect_pass").attr("hidden", false);
            }
        }
    })
});

//отправка номера на сервер
$("#get_password").on('click', function () {
    $("#incorrect_tel").attr("hidden", true);
    if ($("#phone_num").val().length < 10) {
        $("#incorrect_num").attr("hidden", false);
        
        
    }
    else {
        $("#get_password").attr("hidden", true);
        $("#get_password_spinner").attr("hidden", false);
        $('#get_password_spinner').prop('disabled', true);
        $.ajax({
            type: 'GET',
            url: '/Account/Register',
            data: {
                phone: $("#phone_num").val()
            },
            success: function (res) {
                console.log(res); 
                if (res == "bad") {
                    $("#incorrect_tel").attr("hidden", false);
                    $("#get_password").attr("hidden", false);
                    $('#get_password_spinner').prop('disabled', false);
                    $("#get_password_spinner").attr("hidden", true);
                }
                else if (res == "badsms") {
                    $("#sms_error").attr("hidden", false);
                    $("#get_password").attr("hidden", false);
                    $('#get_password_spinner').prop('disabled', false);
                    $("#get_password_spinner").attr("hidden", true);
                }
                else {
                    phoneNumber = $("#phone_num").val();
                    $("#first_reg_form").attr('hidden', true);
                    $("#second_reg_form").attr('hidden', false);
                }
                
            }
        })
    }
});
$('.lk-active').on('click', function(e) {
    e.preventDefault();
    $('.hidden-lk-active-block').toggleClass('active');
});



