using UnityEngine;

namespace HikanyanLaboratory.UISystemTest
{
    public abstract class Screen : MonoBehaviour
    {
        public string Id { get; private set; }

        public void Setup(string id)
        {
            Id = id;
        }

        public virtual void OnInitialize()
        {
        }

        public virtual void OnOpenIn()
        {
        }

        public virtual void OnCloseIn()
        {
        }

        public virtual void OnOpenOut()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnCloseOut()
        {
            gameObject.SetActive(false);
        }
    }
}