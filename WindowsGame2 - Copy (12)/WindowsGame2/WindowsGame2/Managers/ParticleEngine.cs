using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WordGridGame
{
    public class ParticleEngine
    {
        public enum EmitType { Nothing, Small, Explosion, Stars }
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particle> particles;
        private EmitType emitParticles;
        private Shared shared;

        public ParticleEngine(Vector2 location)
        {
            EmitterLocation = location;
            this.particles = new List<Particle>();
            random = new Random();
            emitParticles = 0;
            shared = Shared.Instance;
        }
        public void EmitSmall()
        {
            emitParticles = EmitType.Small;
        }
        public void Update()
        {
            switch (emitParticles)
            {
                case EmitType.Small:
                    for (int i = 0; i < 10; i++)
                    {
                        particles.Add(GenerateNewParticle());
                    }
                    emitParticles = EmitType.Nothing;
                    break;
            }


            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }
        public void EmitBigExplosion()
        {

            emitParticles = EmitType.Explosion;
            for (int i = 0; i < 100; i++)
            {
                particles.Add(NewStarParticle(240, 325));
            }
        }
        public void EmitStars(int x, int y)
        {
            emitParticles = EmitType.Stars;
            for (int i = 0; i < 20; i++)
            {
                particles.Add(NewStarParticle(x, y));
            }
        }
        public void EmitLoadingScreenParticle(Texture2D tex)
        {
            particles.Add(NewLoadingScreenParticle(tex, 240, 350));
        }
        private Particle NewLoadingScreenParticle(Texture2D tex, int x, int y)
        {
            Vector2 position = new Vector2(x, y);
            Vector2 velocity;
            velocity = new Vector2(
                                    6f * (float)(random.NextDouble() * 2 - 1),
                                    6f * (float)(random.NextDouble() * 2 - 1));

            float angle = 0;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = Color.White;
            float size = (float)random.NextDouble();
            int ttl = random.Next(40);
            return new Particle(tex, position, velocity, angle, angularVelocity, color, size, ttl);
        }
        private Particle NewStarParticle(int x, int y)
        {
            Texture2D texture = shared.textureManager.GetTexture("star");
            Vector2 position = new Vector2(x, y);
            Vector2 velocity;
            switch (emitParticles)
            {
                case EmitType.Small:
                    velocity = new Vector2(
                                    3f * (float)(random.NextDouble() * 2 - 1),
                                    3f * (float)(random.NextDouble() * 2 - 1));
                    break;
                case EmitType.Explosion:
                    velocity = new Vector2(
                                    20f * (float)(random.NextDouble() * 2 - 1),
                                    20f * (float)(random.NextDouble() * 2 - 1));
                    break;
                case EmitType.Stars:
                    velocity = new Vector2(
                                    3f * (float)(random.NextDouble() * 2 - 1),
                                    3f * (float)(random.NextDouble() * 2 - 1));
                    break;
                default:
                    velocity = new Vector2(
                                    3f * (float)(random.NextDouble() * 2 - 1),
                                    3f * (float)(random.NextDouble() * 2 - 1));
                    break;
            }

            float angle = 0;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = Color.White;
            float size = (float)random.NextDouble();
            int ttl = random.Next(40);
            return new Particle(shared.textureManager.GetTexture("star"), position, velocity, angle, angularVelocity, color, size, ttl);
        }

        private Particle GenerateNewParticle()
        {
            Texture2D texture = null;

            switch (random.Next(3))
            {
                case 0:
                    texture = shared.textureManager.GetTexture("star");
                    break;
                case 1:
                    texture = shared.textureManager.GetTexture("diamond");
                    break;
                case 2:
                    texture = shared.textureManager.GetTexture("circle");
                    break;
            }

            Vector2 position = EmitterLocation;
            Vector2 velocity = new Vector2(
                                    1f * (float)(random.NextDouble() * 2 - 1),
                                    1f * (float)(random.NextDouble() * 2 - 1));
            float angle = 0;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = new Color(
                        (float)random.NextDouble(),
                        (float)random.NextDouble(),
                        (float)random.NextDouble());
            float size = (float)random.NextDouble();
            int ttl = 20 + random.Next(40);

            return new Particle(texture, position, velocity, angle, angularVelocity, color, size, ttl);
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }

        }
    }
}