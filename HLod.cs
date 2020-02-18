using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HLod : MonoBehaviour
{
	public Renderer testRender;
	public Texture2D TestRenderTexture;

	private TextureCombine tc;
	// Use this for initialization
	private List<Renderer> realItems;
	private bool hlodShowing = false;
	
	void Apply ()
	{
		 GetComponent<MeshRenderer>().enabled = hlodShowing;

		  tc=new TextureCombine();
		realItems=new List<Renderer>();
//		List<Vector4> uvs=new List<Vector4>();
//		testRender.GetComponent<MeshFilter>().sharedMesh.GetUVs(0,uvs);
//			foreach (var uv in uvs)
//		{
//			print(uv);
//		}
//		return;
		
		//filter object 
		Stopwatch sw=new Stopwatch();
		sw.Start();
		List<MeshFilter> meshFilters=new List<MeshFilter>();
		
		foreach (var meshf in FindObjectsOfType<MeshFilter>())
		{
			if ("Plane" == meshf.name || meshf.transform == transform) continue;
			var mr = meshf.GetComponent<Renderer>();
			if(mr==null) continue;
			
			if (Physics.CheckSphere(meshf.transform.position, 0.01f, LayerMask.GetMask("Water")))
			{
				//meshf.gameObject.SetActive(false);
				//mr.enabled = false;
				meshFilters.Add(meshf);
				realItems.Add(mr);
				foreach (var mat in 
					mr.sharedMaterials)
				{
					tc.addTexture(mat.mainTexture as Texture2D);
				}
			}
		}
 	tc.combineAllTextures(2,out TestRenderTexture);
       
      //  return;
		sw.Stop();
		print(sw.ElapsedMilliseconds);
		sw.Reset();
		sw.Start();
		
		Material[] mats=null;
		List<Vector3> vertexList=new List<Vector3>();
		List<Vector3> normalList=new List<Vector3>();
		List<Color> colorList=new List<Color>();
	//	List<Vector4> tangentList=new List<Vector4>();
		List<int> trangleList=new List<int>();
		List<Vector2> uv0List=new List<Vector2>(); 
		List<Vector4> uv1List=new List<Vector4>(); 
		foreach (var meshf in meshFilters)
		{
			 
			
			 
				
				int triCount = meshf.sharedMesh.triangles.Length;
				for (int i = 0; i <triCount ; i++)
				{
					trangleList.Add(vertexList.Count+	meshf.sharedMesh.triangles[i]);
				}

	

			for (int i = 0; i < meshf.sharedMesh.vertexCount; i++)
                				{
                					vertexList.Add(transform.worldToLocalMatrix.MultiplyPoint(  meshf.transform.localToWorldMatrix.MultiplyPoint(meshf.sharedMesh.vertices[i])));
                				}
			
			uv0List.AddRange(meshf.sharedMesh.uv);
			normalList.AddRange(meshf.sharedMesh.normals);
			//tangentList.AddRange(meshf.sharedMesh.tangents);
			 

			
			Vector4[] submeshUv= new Vector4[meshf.sharedMesh.vertexCount];
			Color[] submeshColor= new Color[meshf.sharedMesh.vertexCount];
			 
			for (int i = 0; i < meshf.sharedMesh.subMeshCount; i++)
			{
			int [] triList=	meshf.sharedMesh.GetTriangles(i);
				var uvInHLod=tc.getUvSet(meshf.GetComponent<Renderer>().sharedMaterials[i].mainTexture as Texture2D);
				var color = meshf.GetComponent<Renderer>().sharedMaterials[i].color;
				if(uvInHLod.z*uvInHLod.w==0) continue;
			//	Debug.Log(uvInHLod.x);

				for (int j = 0; j < triList.Length; j++)
				{
					int uvIndex = triList[j];
 					submeshUv[uvIndex]=uvInHLod;
					submeshColor[uvIndex] = color;
				 
				}
			}
			
			uv1List.AddRange(submeshUv);
			colorList.AddRange(submeshColor);
			
		
			 
				
			 

		}
		
		Mesh mesh=new Mesh();
		mesh.SetVertices(vertexList);
		mesh.SetTriangles(  trangleList,0);
		mesh.uv = uv0List.ToArray();
		
		  
	//	mesh.SetTangents( tangentList);
		mesh.SetNormals( normalList);
		mesh.SetColors(colorList);
		//mesh.RecalculateTangents();
		//mesh.RecalculateNormals();
		mesh.SetUVs(1,uv1List);
 		//mesh.UploadMeshData(true);
		GetComponent<MeshFilter>().sharedMesh = mesh;
		sw.Stop();
		print(sw.ElapsedMilliseconds);
		GetComponent<Renderer>().material.shader = Shader.Find("Custom/HLodPbr");
		GetComponent<Renderer>().material.mainTexture = TestRenderTexture;

	}

    private void onTextureCmp()
    {
        TestRenderTexture = tc.target;
    }

    private void Update()
	{
      
		if (realItems == null) return;
		var sqrDis = ((transform.position - Camera.main.transform.position).sqrMagnitude);
		float showDistance = 40*40;
		bool changed = false;
		if (sqrDis > showDistance + 10*10 && hlodShowing == false)
		{
			hlodShowing = true;
			changed = true;
		}
		if (sqrDis < showDistance   && hlodShowing == true)
		{
			hlodShowing = false;
			changed = true;
		}

		if (changed)
		{
			GetComponent<MeshRenderer>().enabled = hlodShowing;
			foreach (var item in realItems)
			{
				item.enabled = !hlodShowing;
			}
		}

	}

	private void OnDestroy()
	{
		if(tc!=null)tc.clear();
	}
 
    private void OnGUI()
	{
		if (realItems != null) return;
		GUI.skin.button.fontSize = 36;
		if (GUI.Button(new Rect(Screen.width/2-200,Screen.height/2,400,40),"实时HLOD模拟离线"))
		{
 
            Apply();

        }
	}
}
