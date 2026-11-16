using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Reflows controls within the safe rectangle, including orientation changes at runtime.</summary>
public class AppResponsiveLayout : MonoBehaviour
{
    Vector2 previous;
    RectTransform[] items;
    RectTransform R(string name)=>items.FirstOrDefault(r=>r.name.Trim()==name);
    void LateUpdate()
    {
        var size=((RectTransform)transform).rect.size;
        if(size!=previous){previous=size;Reflow(size);}
    }
    public void Reflow(Vector2 size)
    {
        items=GetComponentsInChildren<RectTransform>(true);
        bool wide=size.x>size.y*1.15f;
        float w=size.x,h=size.y,pad=40;
        float content=Mathf.Min(w-pad*2,1000);
        string scene=gameObject.scene.name;
        if(scene=="Menu")
        {
            foreach(string page in new[]{"MenuUI","ChoiseMode","Developers"})Page(R(page),wide,w,h,content);
            var nav=items.FirstOrDefault(r=>r.name=="Buttons" && r.parent!=null && r.parent.name=="Information");Page(nav,wide,w,h,content);
            Stack(new[]{R("StartButton"),R("InformationButton"),R("ExitButton")},wide,w,h,content);
            Stack(new[]{R("ModelModeButton"),R("ModeCycleButton"),R("Назад")},wide,w,h,content);
            Stack(new[]{R("InformationPanel1Button"),R("InformationPanel2Button"),R("InformationDevelopersButton")},wide,w,h,content);
            foreach(var rect in items.Where(r=>r.name=="ReturnButton"))P(rect,.5f,0,0,100,content,120);
            var credits=R("Авторы");if(credits!=null)P(credits,wide?.73f:.5f,wide?.54f:.31f,0,0,wide?w*.42f:content,wide?h*.65f:Mathf.Min(600,h*.34f));
            foreach(var scroll in GetComponentsInChildren<ScrollRect>(true))
            {scroll.GetComponent<RectTransform>().offsetMin=new Vector2(pad,200);scroll.GetComponent<RectTransform>().offsetMax=new Vector2(-pad,-100);}
            P(R("ButtonsURL"),wide?.25f:.5f,1,0,-90,540,120);
        }
        else
        {
            P(R("SettingsButton"),1,1,-150,-90,220,112);
            var hint=R("AR подсказка");
            if(hint!=null){hint.anchorMin=new Vector2(0,1);hint.anchorMax=new Vector2(1,1);hint.pivot=new Vector2(.5f,1);hint.offsetMin=new Vector2(pad,-145);hint.offsetMax=new Vector2(-300,-35);}
            P(R("Заголовок настроек"),.5f,1,0,-h*.22f,content,110);
            string[] settings={"ReturnButton","MenuButton","ExitButton"};
            for(int i=0;i<settings.Length;i++)P(R(settings[i]),.5f,.5f,0,100-i*150,Mathf.Min(content,900),125);
            if(scene=="ModelHeart")
            {
                float deck=Mathf.Min(w-2*pad,1100);
                float baseY=wide?380:400;
                P(R("ButtonsRotation"),wide?0:.5f,0,wide?235:-deck*.27f,baseY,400,300);
                P(R("ButtonsScale"),wide?0:.5f,0,wide?235:-deck*.27f,baseY,420,180);
                string[] actions={"SwitchModeButton","ResetButton","InvertButton"};
                for(int i=0;i<3;i++)P(R(actions[i]),wide?1:.5f,0,wide?-270:deck*.26f,baseY+150-i*150,wide?440:deck*.46f,125);
                P(R("PanelTextMode"),wide?1:.5f,0,wide?-270:0,wide?700:710,wide?440:deck,80);
                P(R("Подсказка жестов"),.5f,0,0,wide?880:810,deck,70);
                string[] organs={"ButtonHeart","ButtonDog","ButtonVessels","ButtonLungs"};
                float gap=16,cell=(deck-gap*3)/4;
                for(int i=0;i<4;i++)P(R(organs[i]),.5f,0,(i-1.5f)*(cell+gap),90,cell,120);
                foreach(string name in new[]{"ButtonHideUI","ButtonUnHideUI"})P(R(name),1,1,-150,-220,220,110);
                P(R("Управление дыханием"),wide?1:.5f,0,wide?-350:0,wide?330:340,wide?600:content,300);
                var breathing=R("Управление дыханием");
                if(breathing!=null)
                {
                    float panelWidth=wide?600:content;
                    foreach(Transform child in breathing)
                    {
                        var rect=child as RectTransform;if(rect==null)continue;
                        if(rect.name=="Подпись")P(rect,.5f,1,0,-65,panelWidth-40,85);
                        if(rect.name=="Легенда")P(rect,.5f,0,0,40,panelWidth-40,60);
                        if(rect.name=="Пауза дыхания")P(rect,.5f,.5f,0,-15,Mathf.Min(panelWidth-48,600),108);
                    }
                }
                var heart=R("HeartUI");
                if(heart!=null)
                {
                    float panelWidth=wide?600:content, cx=wide?-350:0,ax=wide?1:.5f;
                    P(R("Фон сердцебиения"),ax,0,cx,400,panelWidth,750);
                    var choices=items.FirstOrDefault(r=>r.name=="Buttons" && r.parent==heart);
                    if(choices!=null){choices.anchorMin=Vector2.zero;choices.anchorMax=Vector2.one;choices.offsetMin=choices.offsetMax=Vector2.zero;}
                    P(R("ButtonPol"),ax,0,cx+panelWidth*.25f,680,panelWidth*.46f,120);
                    var internalButton=items.FirstOrDefault(r=>r.name=="ButtonInt" && r.parent==choices);
                    P(internalButton,ax,0,cx-panelWidth*.25f,680,panelWidth*.46f,120);
                    var texts=R("TextCurrentMode");P(texts,ax,0,cx,530,panelWidth-40,160);
                    P(R("TextPulse"),.5f,.5f,0,40,panelWidth-40,70);
                    P(R("TextMode"),.5f,.5f,0,-40,panelWidth-40,70);
                    P(R("SliderPulse"),ax,0,cx,350,panelWidth-100,80);
                    var sliderRect=R("SliderPulse");var pulseSlider=sliderRect!=null?sliderRect.GetComponent<Slider>():null;
                    if(pulseSlider!=null && pulseSlider.handleRect!=null)pulseSlider.handleRect.sizeDelta=new Vector2(80,0);
                    foreach(string name in new[]{"TextPulse","TextMode","Text0.5","Text2"})
                    {var rect=R(name);if(rect!=null && rect.TryGetComponent<TMPro.TMP_Text>(out var label)){label.enableAutoSizing=true;label.fontSizeMin=28;label.fontSizeMax=42;label.margin=Vector4.zero;label.alignment=TMPro.TextAlignmentOptions.Center;label.textWrappingMode=TMPro.TextWrappingModes.NoWrap;}}
                    var limitText=R("Texts");if(limitText!=null){limitText.localScale=Vector3.one;limitText.anchorMin=Vector2.zero;limitText.anchorMax=Vector2.one;limitText.offsetMin=limitText.offsetMax=Vector2.zero;}
                    P(R("Text0.5"),0,.5f,70,-85,150,60);P(R("Text2"),1,.5f,-70,-85,150,60);
                    foreach(var back in heart.GetComponentsInChildren<Button>(true))if(back.name=="ButtonReturn")P(back.transform as RectTransform,ax,0,cx,100,panelWidth,120);
                }
                var lung=R("LungUI");
                if(lung!=null)foreach(var button in lung.GetComponentsInChildren<Button>(true))if(button.name=="ButtonReturn")P(button.transform as RectTransform,wide?1:.5f,0,wide?-350:0,100,wide?600:content,120);
                var scrim=R("Фон управления");if(scrim!=null)scrim.sizeDelta=new Vector2(0,wide?250:950);
            }
        }
        P(R("SliderLoad"),.5f,.46f,0,0,content,28);
        P(R("Загрузка"),.5f,.58f,0,0,content,100);
    }
    static void Page(RectTransform root,bool wide,float w,float h,float content)
    {
        if(root==null)return;
        foreach(Transform child in root)
        {
            var rect=child as RectTransform;if(rect==null)continue;
            float x=wide?w*.25f:w*.5f;
            float width=wide?w*.42f:content;
            if(rect.name=="Раздел")P(rect,0,1,x,-h*(wide?.25f:.15f),width,65);
            if(rect.name=="Заголовок")P(rect,0,1,x,-h*(wide?.45f:.29f),width,wide?220:280);
            if(rect.name=="Описание")P(rect,0,1,x,-h*(wide?.69f:.45f),width,140);
        }
    }
    static void Stack(RectTransform[] buttons,bool wide,float w,float h,float content)
    {
        float x=wide?w*.73f:w*.5f,width=wide?Mathf.Min(w*.42f,900):content;
        float top=wide?h*.5f+(buttons.Length-1)*82:h*.34f;
        for(int i=0;i<buttons.Length;i++)P(buttons[i],0,0,x,top-i*164,width,136);
    }
    static void P(RectTransform r,float ax,float ay,float x,float y,float w,float h)
    {if(r==null)return;r.localScale=Vector3.one;r.anchorMin=r.anchorMax=new Vector2(ax,ay);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
}
