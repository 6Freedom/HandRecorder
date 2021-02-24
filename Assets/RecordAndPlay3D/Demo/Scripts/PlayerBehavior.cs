using UnityEngine;
using EliCDavis.RecordAndPlay.Record;
using System.Collections.Generic;

namespace EliCDavis.RecordAndPlay.Demo
{
    public class PlayerBehavior : MonoBehaviour
    {

        [SerializeField]
        private Transform bulletSpawn;

        private Recorder recorder;

        private SubjectBehavior subjectBehavior;

        private int bulletsFired;

        public void Initialize(Recorder recorder)
        {
            bulletsFired = 0;
            this.recorder = recorder;
            subjectBehavior = SubjectBehavior.Build(gameObject, recorder, 5, "Player", new Dictionary<string, string>() { { "Bullets Fired", "0" } }, 0.0001f);
        }

        private Vector3 ClampedRotation(Vector3 currentRotation)
        {
            var x = currentRotation.x;

            if (x < 90)
            {
                if (x > 60)
                {
                    x = 60;
                }
            }
            else if (x > 270)
            {
                if (x < 300)
                {
                    x = 300;
                }
            }

            return new Vector3(x, Mathf.Clamp(currentRotation.y, 10, 170), 0);
        }

        private void Fire()
        {
            var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.localScale = Vector3.one * .4f;
            bullet.transform.position = bulletSpawn.position;

            BulletBehavior.Build(SubjectBehavior.Build(bullet, recorder, "Bullet"));

            var rb = bullet.AddComponent<Rigidbody>();
            rb.velocity = transform.forward * 15;
            rb.mass = 3;

            Destroy(bullet, 5);
            bulletsFired ++;
            subjectBehavior.SetMetaData("Bullets Fired", bulletsFired.ToString());
        }

        void Update()
        {
            transform.Rotate(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0);
            transform.eulerAngles = ClampedRotation(transform.eulerAngles);
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Fire();
            }
        }

    }

}