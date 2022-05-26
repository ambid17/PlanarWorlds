using UnityEngine;
using UnityEngine.AI;

namespace Digger
{
    public class NavTest : MonoBehaviour
    {
        public Transform destination;

        private void OnEnable()
        {
            GetComponent<NavMeshAgent>().destination = destination.position;
        }
    }
}