using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class TextureCombine 
{
 private Dictionary<Texture2D, Vector4> uvSet=new Dictionary<Texture2D, Vector4>();
 public Texture2D target;
  public void addTexture(Texture2D texture2D)
 {
  if (texture2D == null) return;
  if(uvSet.ContainsKey(texture2D)==false)uvSet.Add(texture2D,Vector4.zero);
 }
 public void removeTexture(Texture2D texture2D)
 {
  if (texture2D == null) return;
  if(uvSet.ContainsKey(texture2D))uvSet.Remove(texture2D);
 }
 public void clear()
 {
   uvSet.Clear();
  
  }
 

 public Vector4 getUvSet(Texture2D texture2D)
 {
  if(texture2D==null) return  Vector4.zero;
  if(uvSet.ContainsKey(texture2D))return uvSet[texture2D];
  return Vector4.zero;
 }

 
    public void  combineAllTextures(int mipmap,out Texture2D renderTexture)
 {
         
  

  int scale = (int)Mathf.Pow(2, mipmap);
 
  float maxTexutureSize = 1024;

        //  最适合的尺寸 和 摆放方式 需要更好的计算 这里简单 挨个排
        int row = Mathf.CeilToInt(Mathf.Sqrt(uvSet.Count));
        int atlarsSize = ((int)maxTexutureSize) / scale * row;
        target = renderTexture = new Texture2D(atlarsSize, atlarsSize, TextureFormat.DXT1,false);

        
        target.Apply();

 

  
  Vector2 currentPos=Vector2.zero;
 
  List<Texture2D> textList=new List<Texture2D>();
  textList.AddRange(uvSet.Keys);
        int itemindex = -1;
        foreach (var item in textList)
  {
            itemindex++;

             currentPos.x = (itemindex % row) * maxTexutureSize / scale;
            currentPos.y = (itemindex / row) * maxTexutureSize / scale;


            float rtWidth = target.width;
             float rtHeight = target.height;
            int selfAddMip =Mathf.Max(0, (int)Mathf.Log(item.width, 2) - 10);
            int selfAddScale = (int)Mathf.Pow(2, selfAddMip);
            int rectWidth = (int)(item.width/scale/ selfAddScale);
            int rectHeight= (int)(item.height/scale/ selfAddScale);
   Vector4 rect = new Vector4(currentPos.x / rtWidth, currentPos.y / rtHeight,
   rectWidth / rtWidth  ,
   rectHeight / rtHeight );
 
    uvSet[item] = rect;
    Graphics.CopyTexture(item, 0, mipmap+ selfAddMip, 0, 0, rectWidth, rectHeight, target, 0, 0, (int)currentPos.x, (int)currentPos.y);

  }
      //  onCmp();
 // Graphics.SetRenderTarget(null);

    }

 
}
