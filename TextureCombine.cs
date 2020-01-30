using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TextureCombine 
{
 private Dictionary<Texture2D, Vector4> uvSet=new Dictionary<Texture2D, Vector4>();
 private RenderTexture target;
 private static Material combineMat;
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
  if(target!=null) RenderTexture.ReleaseTemporary(target);
  }
 

 public Vector4 getUvSet(Texture2D texture2D)
 {
  if(texture2D==null) return  Vector4.zero;
  if(uvSet.ContainsKey(texture2D))return uvSet[texture2D];
  return Vector4.zero;
 }

  
 public void combineAllTextures(int mipmap, out RenderTexture target)
 {
  if(combineMat==null)combineMat=new Material(Shader.Find("Unlit/TextureCombine"));
  int scale = (int)Mathf.Pow(2, mipmap);
  combineMat.SetFloat("uvScale",scale);
  float maxTexutureSize = 1024;
  //  最适合的尺寸 和 摆放方式 需要更好的计算 这里简单 挨个排
  target = RenderTexture.GetTemporary(256, 256,16,RenderTextureFormat.ARGB32);
  target.useMipMap = true;
  //target.autoGenerateMips = false;
  this.target = target;
  
 
  
  Vector2 currentPos=Vector2.zero;
  //Graphics.SetRenderTarget(target);
  combineMat.SetTexture("BgTex",target);
  List<Texture2D> textList=new List<Texture2D>();
  textList.AddRange(uvSet.Keys);
  foreach (var item in textList)
  {
 
    
   float limitScale =   Mathf.Max(1,item.width/maxTexutureSize,item.height/maxTexutureSize);

   if (currentPos.x + item.width / limitScale / scale > target.width)
   {
    currentPos.x = 0;
    currentPos.y += maxTexutureSize / scale;
    
    
    Debug.Log("reset 0");
   }

   float rtWidth = target.width;
   float rtHeight = target.height;
   Vector4 rect = new Vector4(currentPos.x / rtWidth, currentPos.y / rtHeight,
    item.width / rtWidth / limitScale / scale,
   item.height/rtHeight / limitScale / scale);
  // rect.y = 1 - rect.y-0.25f;
   //item.Value.x = rect.x;//(rect.x,rect.y,rect.z,rect.w);
    uvSet[item] = rect;
   CommandBuffer cb=new CommandBuffer();
   
   cb.Blit(item,target,combineMat);
    combineMat.SetVector("drawRect",rect);
  
   Graphics.ExecuteCommandBuffer(cb);
   cb.Dispose();
   cb.Release();  
   
   ;
// Graphics.DrawTexture(rect, item.Key);
 
//Debug.Log((item.Key.width / limitScale / scale)+","+currentPos.x);
   currentPos.x += item.width / limitScale / scale;
  }
 // Graphics.SetRenderTarget(null);
   
 }

 
}
