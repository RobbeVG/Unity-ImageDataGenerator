// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Single Color ID"
{
    Properties 
    {
        _ID("ID", Color) = (0,0,0,0) //BLACK 
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //CHECK OUT
            ////INCASE OF GPU INSTANCING
            //#include "UnityCG.cginc"

            //UNITY_INSTANCING_BUFFER_START(Props)
            //    UNITY_DEFINE_INSTANCED_PROP(fixed4, _ID)
            //UNITY_INSTANCING_BUFFER_END(Props)

            // vertex shader
            // this time instead of using "appdata" struct, just spell inputs manually,
            // and instead of returning v2f struct, also just return a single output
            // float4 clip position
            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

        //NOT GPU INSTANCING
            //// color from the material
            fixed4 _ID;

            // pixel shader, no inputs needed
            fixed4 frag() : SV_Target
            {
                //UNITY_ACCESS_INSTANCED_PROP(Props, _ID); // just return it
                return _ID;
            }
        ENDCG
        }
    }
}