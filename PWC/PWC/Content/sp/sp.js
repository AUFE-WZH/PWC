var sp_shown = true;
var sp_bodys = [
    
	"online.png"
];
var sp_inited = false;

var sp_news = "";

var sp_words = [
    "大家好，我是AUFEOJ小叮当！~嘻嘻(●'◡'●)",
    "欢迎来到AUFE OnlineJudge!",
    "AUFE OnlineJudge!",
];

var sp_root = "/Content/sp";

var sp_range = sp_bodys.length;
var sp_words_range = sp_words.length;

var now_sp_url = sp_root + "/" + sp_bodys[Math.floor(Math.random() * sp_range)];
var now_sp_word = sp_words[Math.floor(Math.random() * sp_words_range)];

function turn_sp_img()
{
    now_sp_url = sp_root + "/" + sp_bodys[Math.floor(Math.random() * sp_range)];
    $("#sp-img").attr("src", now_sp_url);
}

function turn_sp_word()
{
    now_sp_word = sp_words[Math.floor(Math.random() * sp_words_range)];
    $("#sp-msg").html(now_sp_word);
}

$(function() {

    turn_sp_img();
    if(sp_news!=""){
    	$("#sp-msg").html(sp_news);
    	sp_inited = true;
    	sp_shown = true;
    }else{
        turn_sp_img();
        turn_sp_word();
    }


    $("#sp-shown-hidden").click(function(){
		if(!sp_inited)
		{
			turn_sp_img();
			turn_sp_word();
			
			sp_inited = true;
			
			$("#sp-wrapper").fadeToggle("normal");
		}
		else $("#sp-wrapper").fadeToggle("normal");
        
		$(this).html($(this).html() == "关闭" ? "打开" : "关闭");
		sp_shown = !sp_shown;
		
    });
    
    $("#sp-img").click(function(){
        turn_sp_img();
        turn_sp_word();
    });
});
