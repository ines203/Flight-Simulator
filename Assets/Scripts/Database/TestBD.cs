using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Database
{
    public class TestBD : MonoBehaviour
    {
        private BaseDeDonnees _bd;

        void Start()
        {
            _bd = new BaseDeDonnees();

            var u = _bd.TrouverUtilisateur("Pilote1") ?? _bd.CreerUtilisateur("Pilote1");
            var stats = _bd.GetStats(u.Id);
            stats.PartiesJouees++;
            _bd.Sauvegarder(stats);

            Debug.Log($"{u.Pseudo} : {stats.PartiesJouees} partie(s) jouée(s)");
        }

        void OnDestroy() => _bd?.Fermer();
    }
}
