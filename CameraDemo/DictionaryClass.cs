using PhoneXamlDirect3DApp1Comp;
using System.Collections.Generic;
using System.Windows.Media;
namespace CameraDemo
{
    class DictionaryClass
    {
        /// <summary>
        /// 索引类
        /// </summary>
        private DictionaryClass() { }
         static private DictionaryClass   _dictionaryClass;
         static public DictionaryClass Instance
        {
            get { return _dictionaryClass ?? (_dictionaryClass = new DictionaryClass()); } 
        }

        /// <summary>
        /// 类目数据,如特性,镜头按钮
        /// </summary>
        public Dictionary<string, CategoryClass> CategoryData
        {
            get { return _categoryData; }
        }

        /// <summary>
        /// 文件到content绑定的索引
        /// </summary>
        public Dictionary<string, string> FileToContent
        {
            get { return _fileToContent; }
        }

        /// <summary>
        /// content到EffectType的索引
        /// </summary>
        public Dictionary<string, EffectType> ContentToEffectType
        {
            get { return _contentToEffectType; }
        }   

        /// <summary>
        /// content到ParticleType的索引
        /// </summary>
        public Dictionary<string, KeyValuePair<ParticleType, string>> ContentToParticleType
        {
            get { return _contentToParticleType; }
        }
        

        private Dictionary<string, CategoryClass> _categoryData = new Dictionary<string, CategoryClass>() 
        {
           {"Effect",new CategoryClass(){ Content="特效", Tag="Effect",Foreground=new SolidColorBrush(Colors.Black), Background=new SolidColorBrush(Colors.White)}},
           {"Len", new CategoryClass(){ Content="镜头", Tag="Len",Foreground=new SolidColorBrush(Colors.Black), Background=new SolidColorBrush(Colors.White)}},
           {"Particle", new CategoryClass(){ Content="粒子", Tag="Particle", Foreground=new SolidColorBrush(Colors.Black),Background=new SolidColorBrush(Colors.White)}},
        };


        Dictionary<string, string> _fileToContent = new Dictionary<string, string>() { 
        {"origin.png","原图"},
        //Effect
        {"80S.png","80S"},
        {"abaose.png","阿宝色"},
        {"fugulomo.png","复古风"},
        {"kuai.png","苦艾"},
        {"sumiao.png","素描"},
        {"moran.png","墨染"},
        {"yaogun.png","摇滚"},


        //镜头
        {"press.jpg","挤压"},
        {"double.png","双面镜"},

        //粒子
        {"bullet.png","五彩缤纷"},
        {"snow.png","雪花"},
        {"star.png","星星"},
        {"rain.png","雨"},
        };

        Dictionary<string, EffectType> _contentToEffectType = new Dictionary<string, EffectType>() { 
        {"原图",EffectType.EFFECT_NORMAL},
        //Lomo
        {"80S",EffectType.EFFECT_80S},
        {"阿宝色",EffectType.EFFECT_ABAOSE},
        {"复古风",EffectType.EFFECT_FUGU},
        {"苦艾",EffectType.EFFECT_KUAI},
        {"素描",EffectType.EFFECT_SUMIAO},
        {"墨染",EffectType.EFFECT_MORAN},
        {"摇滚",EffectType.EFFECT_YAOGUN},

        //镜头
        {"挤压",EffectType.EFFECT_OFFSET},
        {"双面镜",EffectType.EFFECT_DBFACE},
        };

        Dictionary<string, KeyValuePair<ParticleType, string>> _contentToParticleType = new Dictionary<string, KeyValuePair<ParticleType, string>>() { 
        {"原图",new  KeyValuePair<ParticleType, string>(ParticleType.PRATICLE_NONE,"")},
        {"雪花",new  KeyValuePair<ParticleType, string>(ParticleType.PRATICLE_SNOW,"Assets/Camera/Particle/Snow.dds")},
        {"五彩缤纷",new  KeyValuePair<ParticleType, string>(ParticleType.PRATICLE_BULLET,"Assets/Camera/Particle/Bullet.dds")},
        {"星星",new  KeyValuePair<ParticleType, string>(ParticleType.PRATICLE_STAR,"Assets/Camera/Particle/LStar.dds")},
        {"雨",new  KeyValuePair<ParticleType, string>(ParticleType.PRATICLE_RAIN,"Assets/Camera/Particle/Rain.dds")},
        };
    }
}
