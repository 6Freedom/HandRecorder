using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EliCDavis.RecordAndPlay.Record;


namespace EliCDavis.RecordAndPlay.Demo
{
    public class BulletBehavior : MonoBehaviour
    {
        SubjectBehavior subject;

        public static BulletBehavior Build(SubjectBehavior subject)
        {
            BulletBehavior bullet = subject.gameObject.AddComponent<BulletBehavior>();
            bullet.subject = subject;
            return bullet;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.transform.name == "Cube")
            {
                var contactPoint = collision.contacts[0].point;
                subject.CaptureCustomEvent("Collision", string.Format("{0} {1} {2}", contactPoint.x, contactPoint.y, contactPoint.z));
            }
        }

    }

}