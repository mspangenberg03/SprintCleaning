using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerPowerUpManager : MonoBehaviour
{
    [Tooltip("How many PowerUps the player can hold")]
    [SerializeField] private int _numberOfPowerUps;
    [SerializeField] private PowerUpBar _PowerUpBar;

    public List<PowerUpBase> _heldPowerUps = new();

    public void PowerUpUsed(PowerUpType _powerUp)
    {
        int index = -1;
        for (int i = 0; i < _heldPowerUps.Count; i++)
        {
            if (_heldPowerUps[i]._type == _powerUp)
            {
                index = i; 
                break;
            }
        }
        if (index == -1)
            throw new System.Exception("No PowerUp in _heldPowerUps is _powerUp.");

        _heldPowerUps[index]._PowerUpUses++;
        if (_heldPowerUps[index]._PowerUpUses == _heldPowerUps[index]._durablity)
        {
            _heldPowerUps.RemoveAt(index);
        }

        _PowerUpBar.UpdateDisplayedInfo(_heldPowerUps);
    }

    public void TryAddPowerUp(PickUpPowerUp pickupPowerUp)
    {
        for (int i = 0; i < _heldPowerUps.Count; i++)
        {
            if (pickupPowerUp.PowerUpInfo._type == _heldPowerUps[i]._type)
            {
                _heldPowerUps[i]._PowerUpUses = 0;
                Destroy(pickupPowerUp.gameObject);
                _PowerUpBar.UpdateDisplayedInfo(_heldPowerUps);
                return;
            }
        }

        _heldPowerUps.Insert(0, pickupPowerUp.PowerUpInfo);
        Destroy(pickupPowerUp.gameObject);

        if (_heldPowerUps.Count > _numberOfPowerUps)
        {
            _heldPowerUps.RemoveAt(_heldPowerUps.Count - 1);
        }

        _PowerUpBar.UpdateDisplayedInfo(_heldPowerUps);
    }

    public bool HasPowerUp(PowerUpType _powerUp)
    {
        foreach (PowerUpBase PowerUp in _heldPowerUps)
        {
            if (PowerUp._type == _powerUp)
                return true;
        }
        return false;
    }
}
