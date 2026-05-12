using UnityEngine;

public class TileParticles : MonoBehaviour
{
    public enum ParticleType { TeleportSource, TeleportDestination, Finish }
    
    [Header("Settings")]
    public ParticleType type;
    
    void Start()
    {
        float height = GetHeightForType();
        CreateParticles(height);
    }
    
    float GetHeightForType()
    {
        switch (type)
        {
            case ParticleType.TeleportSource:  // T - пол, низко
                return 0.15f;
            case ParticleType.TeleportDestination:  // U - стена, выше
                return 1.1f;
            case ParticleType.Finish:  // F - стена, выше
                return 1.1f;
            default:
                return 0.3f;
        }
    }
    
    void CreateParticles(float height)
    {
        GameObject particleSystemObj = new GameObject($"{type}_Particles");
        particleSystemObj.transform.SetParent(transform);
        particleSystemObj.transform.localPosition = new Vector3(0, height, 0);
        
        ParticleSystem ps = particleSystemObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        var emission = ps.emission;
        var shape = ps.shape;
        var colorOverLifetime = ps.colorOverLifetime;
        
        main.loop = true;
        main.startSpeed = 0.4f;
        main.startSize = 0.1f;  // Размер как был изначально
        main.startLifetime = 0.8f;
        main.gravityModifier = 0.02f;
        main.maxParticles = 50;
        
        emission.rateOverTime = 12;  // Ровно 12 частиц в секунду
        
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;
        
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        
        switch (type)
        {
            case ParticleType.TeleportSource:
                gradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(Color.cyan, 0f),
                        new GradientColorKey(Color.blue, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.8f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                break;
            case ParticleType.TeleportDestination:
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.magenta, 0f),
                        new GradientColorKey(new Color(0.5f, 0f, 0.5f), 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.8f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                break;
            case ParticleType.Finish:
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.yellow, 0f),
                        new GradientColorKey(new Color(1f, 0.5f, 0f), 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.8f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                break;
        }
        
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}