(async function()
{
    await CefSharp.BindObjectAsync('worker');

    document.addEventListener('DOMContentLoaded', function (ev) {
        let url = ev.path[0].domain;
        console.log(url)
        worker.domLoaded(url)
    });
})();


var current = 0;
var maxError = 10;


async function easyLogin(login, password) {
    await sleep(3000)
    let loginEl = document.querySelector("input[placeholder='Логин:']")
    let passwordEl = document.querySelector("input[placeholder='Пароль:']")

    loginEl.focus();
    simulate(loginEl, login)

    passwordEl.focus();
    simulate(passwordEl, password)

    let buttonFound = await asyncCallbackHot(() => {
        return !!document.querySelector("#header_container > div.header__main > div.auth-form > div > div > div > button")
    })
    
    if (!buttonFound){
        worker.log("Не нахожу кнопку входа");
        return false;
    }

    let loginButtonEl = document.querySelector("#header_container > div.header__main > div.auth-form > div > div > div > button")

    loginButtonEl.focus();
    simulateClick(loginButtonEl);

    let isCaptcha = false;
    let googleKeyRegex = /\bk=(\w*)&\b/;
    
    while(!isCaptcha) {
        isCaptcha = CheckIsCaptcha();
        if (!isCaptcha ) {
            await sleep(1000);
        }
    }
    
    let frame;
    while (true) {
        frame = document.querySelector("#header_container > div.header__main > div.auth-form > div.recaptcha.auth-form__captcha > div > div > div > iframe");
        
        if (frame) {
            break;
        }
        await sleep(1000);
    }

    let googleKey = googleKeyRegex.exec(frame.src)[1];
    let result = await worker.getGoogleCapcha(googleKey, window.location.href);
    
    setCapcha(___grecaptcha_cfg.clients[0], result);
    return true;
}

function parseMatchData(){
    let fullMatchData = document.querySelector("td.event-description").textContent;
    let dirtyPartials = fullMatchData.replaceAll("\n","").trim().split(" ");
    let scoresArr = [];
    let timeArr = [];
    for(let i = 0;i < dirtyPartials.length;i++){
        if(dirtyPartials[i] !== ""){
            scoresArr.push(dirtyPartials[i])
        }else if(dirtyPartials[i] === ""){
            break;
        }
    }
    timeArr = dirtyPartials.slice(scoresArr.length).filter(e => e !== "");
    let scoresStr = scoresArr.join(" ");
    let timeStr = timeArr.join(" ");
    let timePartials = timeArr.join(" ").split(" ");
    let objTime = {};
    if(timePartials.length === 2){
        objTime = {
            mainTime:timePartials[0],
            additionalTime:timePartials[1].replace("+","")
        }
    }else {
        objTime = {
            mainTime:timePartials[0],
            additionalTime:""
        }
    }
    
    return {
        AdditionalData:scoresStr,
        Time:objTime
    };
}

function setCapcha(fClient, value) {
    for (let key of Object.keys(fClient)) {
        if (fClient[key]) {
            for (let k of Object.keys(fClient[key])) {
                if (fClient[key][k].hasOwnProperty('callback')) {
                    fClient[key][k].callback(value)
                    return
                }
            }
        }
    }
}

function simulateClick(elem) {
    elem.dispatchEvent(new MouseEvent('mousemove', { clientX: 23, clientY: 454 }));
    elem.dispatchEvent(new MouseEvent('mousedown', { clientX: 45, clientY: 54 }));
    
    elem.click();
}

function simulate(elem, newValue) {
    let oldValue = elem.value;

    let event = new Event('input', { bubbles: true });
    event.simulated = true;

    elem.value = newValue;

    let tracker = elem._valueTracker;
    if (tracker) {
        tracker.setValue(oldValue);
    }

    elem.dispatchEvent(new KeyboardEvent('keydown', { bubbles: true }));
    elem.dispatchEvent(new KeyboardEvent('keypress', { bubbles: true }));
    elem.dispatchEvent(new KeyboardEvent('keyup', { bubbles: true }));
    elem.dispatchEvent(new Event('input', { bubbles: true }));
    elem.dispatchEvent(new Event('change', { bubbles: true }));
}

async function findStakeAndClick(betId){
    if (document.getElementById('broadcastWidgetContainer')) {
        document.getElementById('broadcastWidgetContainer').style.display = 'none';
    }
    if (getBetslip()._betCalc) {
        getBetslip().removeAll();
        let isClearedCoupon = await asyncCallbackHot(() => {
            return getStakeCount() === 0;
        });
        if (!isClearedCoupon) {
            //worker.logi("Не смог очистить купон в марафоне")
            return false;
        }
    }
    if (!BetslipStorageFacade.controller.isOpenCoupon())//нативная функция Марафона,которая проверяет открыт ли купон
        toggleBetslip();


    if (document.getElementById('coupon_body'))//раскрывает купон
        document.getElementById('coupon_body').style.display = 'table';

    let stake = document.querySelector("span[data-selection-key='" + betId + "']");

    if (!stake) {
        //worker.logi("Не нашёл ставку в марафоне")
        return false;
    }
    stake.click();

    let isStakeInCoupon = await asyncCallbackHot(() => {
        return getStakeCount() === 1;
    });

    if (!isStakeInCoupon) {
        //worker.logi("Ставка не попала в купон в марафоне");
        return false;
    }

    document.getElementById('betPlacingModeRadio_GreaterOrEqualsToCurrent').click();
    return true;
}

function setStakeSum(sumStake) {
    try {
        var y = $('input[name="stake"]')[0];
        simulate(y,sumStake);
        var dt = y.id.replace('stake.', '');
        getBetslip().checkBet(dt);//проверяет ставку на соответствие с текущей выбранной ставкой
        return true;
    } catch (ex) {
        worker.log("не удалось установить сумму ставки")
        return false;
    }
}

function doStake() {
    try {
        return getBetslip().confirmPlaceBet(true, true);
    } catch (e) {
        worker.log("Не удалось сделать ставку")
    }
}

async function checkWindowResult(){
    try{
        await sleep(3000)
        let progressDialog = await asyncCallbackLong(()=>{
            return isInView(document.querySelector("#progress_dialog > img"))
        },1000,10);
        var isOpenWindowWithResultAndClosedProgressDialog = await asyncCallbackLong(() => {
            return WindowResultOpenAndProgressDialogClose()
        },1000,10);
        if(isOpenWindowWithResultAndClosedProgressDialog){
            worker.couponWindowResultCallback("marathonCouponWindow");
        }
    }catch (e) {
        worker.logi("При проверке окна результата на марафоне произошла ошибка")
    }
}
function WindowResultOpenAndProgressDialogClose(){
    let isActiveProgressDialog = isInView(document.querySelector("#progress_dialog > img"));
    let textInWorker = isActiveProgressDialog === true? "Да":"Нет";
    worker.logi("Ставка еще ставится на марафоне? " + textInWorker);
    
    let windowBlock = isInView(document.getElementById('betresult'));
    let progressDialog = isInView(document.querySelector("#progress_dialog > img"));
    return windowBlock && !progressDialog && !isActiveProgressDialog;
}

async function checkWindowResultTextSuccess(){
    try{
        let nowOpenStakeCount = parseInt(document.getElementById('open_bets_count').innerText);//количество открытых пари
        worker.logi("Количество открытых ставок сейчас в Марафоне: " + nowOpenStakeCount);
        let isPerfectBetStatusText = (document.getElementById('betresult').innerText === 'Ваша ставка принята, спасибо' || document.getElementById('betresult').innerText === 'Пари принято, спасибо');
        worker.logi("Текст ставки в конторе Марафон:" + document.getElementById('betresult').innerText);
        let resultStake = (isInView(document.getElementById('ok-button')) && isPerfectBetStatusText);
        if (resultStake) {
            worker.logi('Ставка в Марафоне успешно принята');
        }
        if (!resultStake && isInView(document.getElementById('cancel-button'))) {
            document.getElementById('cancel-button').click();
        }
        return resultStake;
    }catch(e){
        worker.logi("При проверке на Марафоне результата окна текста произошла ошибка");
        return false;
    }
}



async function CheckInit() {
    try {
        worker.Islogin = CheckLogin();
        if (worker.Islogin) {
          console.log('Статус авторизации: ' + CheckLogin());
            //worker.JSLogined();
            var balance = getBalance();
           console.log('Баланс в Marafon`e: ' + balance);
            //worker.JSBalanceChange(balance);
            return;
        }
       
        console.log('Статус авторизации: ' + CheckLogin());
        console.log('Авторизация');
        let loginField = document.querySelector("input[placeholder='Логин:']");
        simulate(loginField, worker.login);
        let passwordField = document.querySelector("input[placeholder='Пароль:']");
        simulate(passwordField, worker.password);
        let button = document.querySelectorAll("button[type='button']")[1];
        console.log(!!button);
        button.click();
        button.click();
        button.click();
        button.click();
        button.click();
        button.click();
        let googleKeyRegex = /\bk=(\w*)&\b/;
        //setTimeout(async function () {
        //    if (CheckIsCaptcha()) {
        //        console.log('Требуется каптча');
        //        console.log(123);
        //        let taskId = null;
        //        let frame = document.querySelector("#header_container > div.header__main > div.auth-form > div.recaptcha.auth-form__captcha > div > div > div > iframe");
        //        let googleKey = googleKeyRegex.exec(frame.src)[1];
        //        try {
        //            taskId = worker.capcha.startResolveGoogleCaptcha(googleKey, window.location.href);
        //            console.log("taskID");
        //            console.log(taskId);
        //        } catch (ex) {
        //            console.log("не удалось получить taskId");
        //            console.log("причина " + ex);
        //        }
        //        let count = 1;
        //        let timer = setInterval(() => { console.log(count++) }, 1000);
        //        await sleep(30000);
        //        let capchaResponse = null;
        //        for (let i = 0; i < 10; i++) {
        //            capchaResponse = worker.capcha.getGoogleCaptchaResponse(taskId);
        //            if (capchaResponse.indexOf("error") < 0) {
        //                break;
        //            } else {
        //                console.log("капча не разгадана");
        //                await sleep(10000);
        //            }
        //        }
        //        ___grecaptcha_cfg.clients[0].l.l.callback(capchaResponse)
        //    }
        //}, 10000);

    } catch (e) {
        current++;
        if (current >= maxError) {
           console.log('CheckInit: return');
            return;
        }
        setTimeout(CheckInit, 500);
    }
}

function CheckIsCaptcha() {
    try {
        return !!document.querySelector("iframe[title='reCAPTCHA']");
    } catch (e) {
        return false;
    }
}

function getStakeInfo() {
    if (!BetslipStorageFacade.controller.isOpenCoupon())
        toggleBetslip();

   console.log('getStakeInfo from Marafon');
   // if (!worker.stakeInfo) {
   //    console.log('StakeInfo error');
   // }
   //// worker.stakeInfo.Auth = CheckLogin();
   // //worker.StakeInfo.StakeCount = getStakeCount();

   // worker.StakeInfo.Balance = parseFloat(getBalance());
   // worker.StakeInfo.MinSumm = parseFloat(getMinSum());
   // worker.StakeInfo.MaxSumm = parseFloat(getMaxSum());
   // worker.StakeInfo.Summ = parseFloat(getSum());

   // if (worker.StakeInfo.StakeCount === 1) {
   //     worker.StakeInfo.IsEnabled = getStakeEnabled();

   //     worker.StakeInfo.Coef = getCoefFromCoupon();

   //     worker.StakeInfo.Parametr = parseFloat(getParameterFromCoupon());
   // }


  /*  return JSON.stringify(worker.StakeInfo);*/
}

function getParameterFromCoupon() {
//    //try {
//    //    let stakeInfo = document.querySelector('#single-container div[id^="selection.name."]');
//    //    //if (!stakeInfo) {
//    //        console.log('Не могу определить параметр в купоне');
//    //       // return -6666;
//    //  //  }

//        if (stakeInfo.id.indexOf('Under_') !== -1 || stakeInfo.id.indexOf('Over_') !== -1) {
//            var str1 = stakeInfo.innerText.trim().split(' ');
//            if (str1.length > 1 && str1[str1.length - 1].indexOf(',') !== -1) {
//                console.log('Обнаружен азиатский тотал');
//                let dt = str1[str1.length - 1].split(',');
//                if (dt.length !== 2) {
//                    console.log('Ошибка извлечения азиатского тотала: ' + str1);
//                    return -6666;
//                }
//                let total1 = parseFloat(dt[0]);
//                let total2 = parseFloat(dt[1]);
//                return (total1 + total2) / 2;
//            } else
//                return parseFloat(str1[str1.length - 1].trim());
//        }
//        let foraReg = new RegExp(/\(.*?\)/);
//        if (!foraReg.test(stakeInfo.innerText)) {
//            console.log('Ставка без параметра');
//            return -6666;
//        }

//        console.log('Обнаружен ставка с параметром');
//        var rez = foraReg.exec(stakeInfo.innerText)[0].replace('(', '').replace(')', '').trim();
//        if (rez.indexOf(',') !== -1) {
//            console.log('Обнаружена азиатская фора');
//            let dt = rez.split(',');
//            if (dt.length !== 2) {
//                console.log('Ошибка извлечения азиатской форы: ' + rez);
//                return -6666;
//            }
//            let fora1 = parseFloat(dt[0]);
//            let fora2 = parseFloat(dt[1]);
//            return (fora1 + fora2) / 2;
//        }
//        return parseFloat(rez);
//    } catch (e) {
//        return -6666;
//    }
}

function getCoefFromCoupon() {
    try {
        return parseFloat(window.document.querySelector('#single-container span[id^="price.presentation."]').innerText);
    } catch (e) {
       console.log('Ошибка определения кэфа. его нет в купоне');
        return 0;
    }
}

function getStakeEnabled() {
    try {
        if (document.getElementById('coupon_body').getBoundingClientRect().width === 0) {
           helper.writeLine('Купон закрыт! Ставка может быть не доступна!');
            return false;
        }
        return !isInView(document.querySelector('#single-container div[id^="display.is_not_active."]'));
    } catch (e) {
        return false;
    }
}

function getSum() {
    try {
        return window.document.querySelector('#single-container input[id^="stake."]').value;
    } catch (e) {
        return 0;
    }
}

function getMinSum() {
    try {
        return window.document.querySelector('#single-container span[id^="min-stake-"]').innerText.trim();
    } catch (e) {
        console.log('Ошибка определения мин.Суммы. ее нет');
        return 0;
    }
}

function getMaxSum() {
    try {
        return window.document.querySelector('#single-container span[id^="max-stake-"]').innerText.trim().replace(',', '');
    } catch (e) {
        console.log('Ошибка определения макс.Суммы. ее нет');
        return 0;
    }
}

function getStakeCount() {
    return window.document.querySelectorAll('#single-container td.del').length;
}

function getBalance() {
    try {
        var balDt = document.querySelector('[data-punter-balance-type="main"]').innerText.replace(',', '');
        var balance = parseFloat(balDt);
        return balance;
    } catch (ex) {
       console.log('Не смогли спарсить баланс. Его нету');
        return -1;
    }
}

function CheckLogin() {
    if (document.getElementById('logoutLink') && !document.getElementById('auth_login'))
        return true;
    return false;
}

function couponOpen(ch) {
    BetslipStorageFacade.controller.setIsBetslipActive(!0);
    betslipAjaxPost({
        postAction: ADD_URL,
        data: {
            // ch: JSON.stringify(ch),
            ch: ch,
            url: "",
            ws: !0
        },
        success: function (e) {
            loadBetslipContent(e);
            TrackingEngine.trigger(TrackingEngine.EVENT.BETSLIP_CHOICE_ADDED, ch.u);
        }
    })
}

async function ShowStake() {
    // let isLoadLine = await asyncCallbackHot(() => {
    //     if (document.querySelector('td[data-sel]'))
    //         return true;
    //     return false;
    // });
    // if (!isLoadLine) {
    //     console.log('Линия так и не загрузилась');
    //     worker.JSFail();
    //     return;
    // }
    console.log('Show stake Марафон Старт');
    let isLoadBasket = await asyncCallbackHot(() => {
        if (document.getElementById('coupon_body'))
            return true;
        return false;
    });
    console.log("1 Марафон");
    if (!isLoadBasket) {
       console.log('Купон так и не загрузился 286');
       // worker.JSFail();
        return;
    }
    console.log("2 Марафон");
    if (document.getElementById('broadcastWidgetContainer')) {
        document.getElementById('broadcastWidgetContainer').style.display = 'none';
    }
    console.log("3 Марафон");
    if (getBetslip()._betCalc) {
       console.log('В купоне есть ставки. Чистим 295');
        getBetslip().removeAll();
        let isClearedCoupon = await asyncCallbackHot(() => {
            return getStakeCount() === 0;
        });
        if (!isClearedCoupon) {
           console.log('Купон так и не очистился 301');
           // worker.JSFail();
            return;
        }
    }
    console.log("4 Марафон");
    hideWidjet();
    if (document.getElementById('coupon_body'))
        document.getElementById('coupon_body').style.display = 'table';
    if (!BetslipStorageFacade.controller.isOpenCoupon())
        toggleBetslip();
    console.log("5 Марафон");

    let stake = document.querySelector("span[data-selection-key='" + worker.betId + "']");
    if (!stake) {
       console.log('Нет такой ставки в линии 316');
    //    worker.JSFail();
        return;
    }
    
    stake.click();
    // couponOpen(worker.betId);
    let isStakeInCoupon = await asyncCallbackHot(() => {
        return getStakeCount() === 1;
    });

    if (!isStakeInCoupon) {
       console.log('Ставка так и не попала в купон 328');
       // worker.JSFail();
        return;
    }

    worker.Islogin = CheckLogin();
   // worker.JSLogined();
    if (worker.Islogin) {
        var balance = getBalance();
       // worker.JSBalanceChange(balance);
    }
    //радио кнопка для установки подтверждения при изменении коэффициента
    document.getElementById('betPlacingModeRadio_GreaterOrEqualsToCurrent').click();
    if (worker.IsValueStrategy)
        document.getElementById('betPlacingModeRadio_Any').click();
    return true;
   // worker.JSStop();
}

function hideWidjet() {
    var headers = document.querySelectorAll('td.broadcast-header-mode');
    for (var i = 0; i < headers.length; i++) {
        if (headers[i].classList.contains('broadcast-header-mode-selected'))
            headers[i].click();
    }
}

let isNowPlaced = false;
var currentOpenStakeCount;

let sleepCounter = 0;

//Проверяем принята ли ставка


async function FastLoad() {
  console.log("asdadsfsd")
  console.log('Новая загрузка');
    if (isInView(document.getElementById('simplemodal-overlay'))) {
      console.log('Обнаружено модальное окно - закрываем его');
        closeModal();
        let isClosedModal = await asyncCallbackHot(() => {
            return !isInView(document.getElementById('simplemodal-overlay'));
        });
        if (!isClosedModal) {
           console.log('Не смогли закрыть модальное окно. Делаем полную загрузку');
            //window.location.href = worker.EventUrl;
            return;
        }
    }
    
     let letCurrentTeam = document.querySelector('a[href^="/su/live/' + worker.EventId + '"]');
     if (!letCurrentTeam) {
         console.log('Не смогли найти нужное событие. Делаем полную загрузку');
         window.location.href = worker.EventUrl;
         return;
     }
    
     let teams = letCurrentTeam.textContent;
     console.log('Открываем ' + letCurrentTeam.textContent);
     letCurrentTeam.click();
     let isLoadCurrentTeam = await asyncCallbackHot(() => {
         let teamsSpan = document.querySelectorAll('[data-members="true"]')[0];
         if (teamsSpan.children.length !== 2)
             return false;
         if (teams.indexOf(teamsSpan.children[0].textContent) !== -1 && teams.indexOf(teamsSpan.children[1].textContent) !== -1)
             return true;
         return false;
     });
     if (!isLoadCurrentTeam) {
         console.log('Нужная команда так и не загрузилась');
         window.location.href = worker.EventUrl;
         return;
     }
    //ShowStake();
}

async function sleep(msec) {
    return new Promise(resolve => setTimeout(resolve, msec));
}

async function asyncCallback(callbackBoolean) {
    var tryCounter = 0;
    while (tryCounter < 10) {
        if (callbackBoolean()) {
            return true;
        }
        tryCounter++;
        await sleep(1000);
    }
    return false;
}

async function asyncCallbackLong(callbackBoolean, sleepTime, tryCount) {
    var tryCounter = 0;
    while (tryCounter < tryCount) {
        if (callbackBoolean()) {
            return true;
        }
        tryCounter++;
        await sleep(sleepTime);
    }
    return false;
}

async function asyncCallbackHot(callbackBoolean) {
    return await asyncCallbackLong(callbackBoolean, 200, 50);
}

function isInView(element) {
    if (!element) return false;
    return element.getBoundingClientRect().width !== 0;
}

//getBetslip()._betCalc==null
// = true - если нет открытых ставок
//getBetslip().removeAll();