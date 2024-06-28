using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsSelector : MonoBehaviour
{
    [SerializeField] private int _minProps;
    [SerializeField] private int _maxProps;
    [SerializeField] private Prop[] _props;

    private List<Prop> _selectedProps = new();
    private List<Prop> _optionsLeft = new();


    private void OnEnable()
    {
        do
        {
            foreach (Prop p in _props)
                p.gameObject.SetActive(false);

            int r = Random.Range(_minProps, _maxProps);

            _selectedProps.Clear();
            _optionsLeft.Clear();
            foreach (Prop p in _props)
                _optionsLeft.Add(p);
            for (int i = 0; i < r; i++)
                _selectedProps.Add(_optionsLeft.TakeRandomElement());
        } while (PropsArentAllCompatible());

        foreach (Prop p in _selectedProps)
            p.gameObject.SetActive(true);
    }

    private bool PropsArentAllCompatible()
    {
        foreach (Prop p in _selectedProps)
        {
            foreach (Prop p2 in p.IncompatibleProps)
            {
                if (_selectedProps.Contains(p2))
                    return true;
            }
        }
        return false;
    }



}
