using UnityEngine;

public class DrawManager : MonoBehaviour
{
    public Material mat;

    private void OnPostRender()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }
        GL.PushMatrix(); //保存当前Matirx
        mat.SetPass(0); //刷新当前材质
        GL.LoadPixelMatrix();//设置pixelMatrix
        GL.Color(Color.yellow);
        GL.Begin(GL.LINES);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(Screen.width, Screen.height, 0);
        GL.End();
        GL.PopMatrix();//读取之前的Matrix
    }
}