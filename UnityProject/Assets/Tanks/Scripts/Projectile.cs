using FishNet.Object;
using UnityEngine;

namespace FishNet.Examples.Tanks
{
    public class Projectile : NetworkBehaviour
    {
        public float destroyAfter = 5;
        public Rigidbody rigidBody;
        public float force = 1000;

        // set velocity for server and client. this way we don't have to sync the
        // position, because both the server and the client simulate it.
        void Start()
        {
            rigidBody.AddForce(transform.forward * force);
        }

        // destroy for everyone on the server
        [Server]
        void DestroySelf()
        {
            Despawn(gameObject);
        }

        // ServerCallback because we don't want a warning if OnTriggerEnter is
        // called on the client

        void OnTriggerEnter(Collider co)
        {
            Despawn(gameObject);
        }
    }
}
