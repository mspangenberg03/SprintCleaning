using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerPowerUpManager : MonoBehaviour
{
    [Tooltip("How many PowerUps the player can hold")]
    [SerializeField] private int _numberOfPowerUps;
    [SerializeField] private PowerUpBar _PowerUpBar;
    [SerializeField] private PlayerMovement _PlayerMovement;

    [SerializeField] private GameObject _Vaccum;

    public List<PowerUpBase> _heldPowerUps = new();

    public void PowerUpUsed(PowerUpType _powerUp)
    {
        /*
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
        */
    }

    public void TryAddPowerUp(PickUpPowerUp pickupPowerUp)
    {
        for (int i = 0; i < _heldPowerUps.Count; i++)
        {
            if (pickupPowerUp.PowerUpInfo._type == _heldPowerUps[i]._type)
            {
                _heldPowerUps[i]._PowerUpTimer = 0f;
                Destroy(pickupPowerUp.gameObject);
                _PowerUpBar.UpdateDisplayedInfo(_heldPowerUps);
                return;
            }
        }

        _heldPowerUps.Insert(0, pickupPowerUp.PowerUpInfo);
        if(pickupPowerUp.PowerUpInfo._type == PowerUpType.Vaccum){
           CapsuleCollider _vacCol = _Vaccum.GetComponent<CapsuleCollider>();
           _vacCol.enabled = true;
        }
        else if(pickupPowerUp.PowerUpInfo._type == PowerUpType.Speed_Boots){
            _PlayerMovement.ChangeSpeedMult(1.5f);
        }
        /*else if(pickupPowerUp.PowerUpInfo._type == PowerUpType.Score_Mult){

        }
        */
        Destroy(pickupPowerUp.gameObject);
        


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
    public void PowerUpAfterUpdate()
    {
        int index = -1;
        for (int i = 0; i < _heldPowerUps.Count; i++)
        {
            _heldPowerUps[i]._PowerUpTimer = _heldPowerUps[i]._PowerUpTimer + Time.deltaTime;
            if (_heldPowerUps[i]._PowerUpTimer >= _heldPowerUps[i]._length){
                index = i;
                if(_heldPowerUps[i]._type == PowerUpType.Vaccum){
                    CapsuleCollider _vacCol = _Vaccum.GetComponent<CapsuleCollider>();
                    _vacCol.enabled = false;
                    Debug.Log("vac done");
                }
                else if(_heldPowerUps[i]._type == PowerUpType.Speed_Boots){
                    _PlayerMovement.ChangeSpeedMult(1f);
                    Debug.Log("speed done");
                }
                /*else if(_heldPowerUps[i].PowerUpInfo._type == PowerUpType.Score_Mult){

                }
                */
            }
        }
        if(index != -1){
            _heldPowerUps.RemoveAt(index);
        }       
    }
}
