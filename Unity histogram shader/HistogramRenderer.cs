using UnityEngine;

public class HistogramRenderer : MonoBehaviour
{
    [SerializeField] private ComputeShader _computeShader;
    private ComputeBuffer _colFreqsBuffer;
    private uint[] _colFreqs;
    [SerializeField] private Shader _eqShader;

    private int initHandler;
    private int kernelHandler;

    [SerializeField] private int _lowResFactor = 2;
    [Range(0, 1)] [SerializeField] private float _brightness;
    private void Awake()
    {
        _colFreqs = new uint[256 * 4];
        
        kernelHandler = _computeShader.FindKernel("CSMain");
        initHandler = _computeShader.FindKernel("CSInit");

        _colFreqsBuffer = new ComputeBuffer(256, sizeof(uint) * 4);
        _computeShader.SetBuffer(initHandler, "colFreqs", _colFreqsBuffer);
        _computeShader.SetBuffer(kernelHandler, "colFreqs", _colFreqsBuffer);
    }

    private float[] Floatize(uint[] freqs)
    {
        float[] f = new float[256*4];
        for(int i = 0; i < 256*4; i++)
        {
            f[i] = freqs[i];
        }
        return f;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
        source.filterMode = FilterMode.Point;
        RenderTexture lowRes = new RenderTexture(source.width / _lowResFactor, source.height / _lowResFactor, 3);
        lowRes.filterMode = FilterMode.Point;
        Graphics.Blit(source, lowRes);
        //Graphics.Blit(lowRes, destination);
        _computeShader.SetTexture(kernelHandler, "source", lowRes);
        _computeShader.Dispatch(initHandler, 256 / 64, 1, 1);
        _computeShader.Dispatch(kernelHandler, lowRes.width/8, lowRes.height/8, 1);

        _colFreqsBuffer.GetData(_colFreqs);

        Material eqMat = new Material(_eqShader);
        eqMat.SetTexture("_MainTex", source);
        eqMat.SetFloatArray("_CProps", Floatize(_colFreqs));
        eqMat.SetInt("_Size", source.width * source.height);
        eqMat.SetFloat("_Brightness", _brightness);

        Graphics.Blit(source, destination, eqMat);
    }
}