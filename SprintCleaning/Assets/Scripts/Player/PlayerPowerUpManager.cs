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
    [SerializeField] private GameObject _Score;

    public List<PowerUpBase> _heldPowerUps = new();

    

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
        else if(pickupPowerUp.PowerUpInfo._type == PowerUpType.Score_Mult){
            ScoreManager _scoreManager = _Score.GetComponent<ScoreManager>();
            _scoreManager._powerUpMultiplier = 2;
        }
        
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
                }
                else if(_heldPowerUps[i]._type == PowerUpType.Speed_Boots){
                    _PlayerMovement.ChangeSpeedMult(1f);
                }
                else if(_heldPowerUps[i]._type == PowerUpType.Score_Mult){
                    ScoreManager _scoreManager = _Score.GetComponent<ScoreManager>();
                    _scoreManager._powerUpMultiplier = 1;
                }
                
            }
        }
        if(index != -1){
            _heldPowerUps.RemoveAt(index);
        }       
    }
}
