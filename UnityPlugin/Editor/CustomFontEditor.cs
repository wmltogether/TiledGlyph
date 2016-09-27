#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.IO;
using System.Xml;

public static class CustomFontEditor
{
    public struct FontInfo
    {
        public int index;
        public int x;
        public int y;
        public int width;
        public int height;
        public FontInfo(int a, int b, int c, int d, int e)
        {
            index = a;
            x = b;
            y = c;
            width = d;
            height = e;
        }
    }

    [MenuItem("Assets/根据BTNF格式创建Unity字库")]
    public static void GenerateFont()
    {
        TextAsset selected = (TextAsset)Selection.activeObject;
        string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selected));

        Texture2D texture = AssetDatabase.LoadAssetAtPath(rootPath + "/" + selected.name + ".png", typeof(Texture2D)) as Texture2D;
        if (!texture)
            throw new UnityException("Texture2d asset doesn't exist for " + selected.name);

        string exportPath = rootPath + "/" + Path.GetFileNameWithoutExtension(selected.name);

        customBuilder(selected, exportPath, texture);
    }

    private static void customBuilder(TextAsset import, string exportPath, Texture2D texture)
    {
        if (!import)
            throw new UnityException(import.name + "is not a valid font file");
        Stream ms = new MemoryStream(import.bytes);
        BinaryReader br = new BinaryReader(ms);
        long sig = br.ReadInt32();
        if (sig != 0x42544e46)// BTNF header 
            throw new UnityException(import.name + "is not a valid BTNF fontfile");
        float texW = texture.width;
        float texH = texture.height;
        int char_nums = br.ReadInt32();
        float m_FontSize = 0;
        Rect r;
        CharacterInfo[] charInfos = new CharacterInfo[char_nums];
        for (int i = 0; i < char_nums; i++)
        {
            FontInfo mFontInfo = new FontInfo(br.ReadInt32(), 
                                              br.ReadInt32(), 
                                              br.ReadInt32(), 
                                              br.ReadInt32(), 
                                              br.ReadInt32());
            br.ReadInt32();
            CharacterInfo charInfo = new CharacterInfo();
            r = new Rect();
            r.x = ((float)mFontInfo.x) / texW;
            r.y = ((float)mFontInfo.y) / texH;
            r.width = ((float)mFontInfo.width) / texW;
            r.height = ((float)mFontInfo.height) / texH;
            r.y = 1f - r.y - r.height;
            charInfo.uvBottomLeft = new Vector2(r.xMin, r.yMin);
            charInfo.uvBottomRight = new Vector2(r.xMax, r.yMin);
            charInfo.uvTopLeft = new Vector2(r.xMin, r.yMax);
            charInfo.uvTopRight = new Vector2(r.xMax, r.yMax);
            charInfo.index = (int)mFontInfo.index;
            charInfo.advance = (int)mFontInfo.width;
            
            m_FontSize = (float)mFontInfo.height;
            r = new Rect();
            r.x = (float)0f;
            r.y = (float)0f;
            r.width = (float)mFontInfo.width;
            r.height = (float)mFontInfo.height;
            r.y = -r.y;
            r.height = -r.height;
            charInfo.minX = (int)r.xMin;
            charInfo.maxX = (int)r.xMax;
            charInfo.minY = (int)r.yMax;
            charInfo.maxY = (int)r.yMin;

            charInfos[i] = charInfo;


        }

        br.Close();
        ms.Close();

        Shader shader = Shader.Find("UI/Default");
        Material material = new Material(shader);
        material.mainTexture = texture;
        AssetDatabase.CreateAsset(material, exportPath + ".mat");

        // Create font
        Font font = new Font();
        font.material = material;
        font.name = import.name;
        font.characterInfo = charInfos;

        SerializedObject mFont = new SerializedObject(font);
        mFont.FindProperty("m_FontSize").floatValue = m_FontSize - 2f;
        mFont.FindProperty("m_LineSpacing").floatValue = m_FontSize + 2f;
        mFont.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(font, exportPath + ".fontsettings");
        Debug.Log(String.Format("创建自定义字库完成！{0}", import.name));
    }

    private static float ToFloat(XmlNode node, string name)
    {
        return float.Parse(node.Attributes.GetNamedItem(name).InnerText);
    }
}
#endif
