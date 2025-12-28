using System.Collections;
using UnityEngine;

namespace Netologia.TowerDefence
{
	public class UnitVisual : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer _renderer;
		[SerializeField]
		private Sprite[] _sprites;
		public bool visualWait;
		public bool damageDealt { get; set; }
		public float deathTimer = 1f;

		public void ManualUpdate(float delta, bool direction = false)
		{
			if (damageDealt)
			{
                _renderer.sprite = _sprites[0];
				damageDealt = false;
				StartCoroutine("DamageAnimation");
			}

			if (direction) _renderer.flipX = !_renderer.flipX;
		}

		IEnumerator DamageAnimation()
		{
			yield return new WaitForSeconds(1);
            _renderer.sprite = _sprites[1];
        }

		public void ShowDeath()
		{
            StartCoroutine("DeathAnimation");
        }

        IEnumerator DeathAnimation()
        {
            _renderer.sprite = _sprites[2];
            yield return new WaitForSeconds(1);
            _renderer.sprite = _sprites[1];
			deathTimer = 1f;
        }
    }
}