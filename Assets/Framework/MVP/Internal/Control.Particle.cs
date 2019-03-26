namespace Framework.MVP.Internal
{
    internal class Particle : Control<UnityEngine.ParticleSystem>, IParticle
    {
        public void Play() => Component.Play();
    }
}
