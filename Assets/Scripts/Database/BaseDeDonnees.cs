using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Database
{
    using System.Collections.Generic;
    using System.IO;
    using SQLite;
    using UnityEngine;

    public class BaseDeDonnees
    {
        private readonly SQLiteConnection _db;

        public BaseDeDonnees()
        {
            string chemin = Path.Combine(Application.persistentDataPath, "jeu.db");
            Debug.Log("Base de données : " + chemin);

            _db = new SQLiteConnection(chemin);
            _db.CreateTable<Utilisateur>();
            _db.CreateTable<Statistiques>();
        }

        public Utilisateur CreerUtilisateur(string pseudo)
        {
            var u = new Utilisateur { Pseudo = pseudo };
            _db.Insert(u); 
            _db.Insert(new Statistiques { UtilisateurId = u.Id });
            return u;
        }

        public Utilisateur TrouverUtilisateur(string pseudo) =>
            _db.Table<Utilisateur>().FirstOrDefault(u => u.Pseudo == pseudo);

        public Statistiques GetStats(int utilisateurId) =>
            _db.Table<Statistiques>().First(s => s.UtilisateurId == utilisateurId);

        public void Sauvegarder(Statistiques s) => _db.Update(s);

        public List<Utilisateur> TousLesUtilisateurs() =>
            _db.Table<Utilisateur>().ToList();

        public void Fermer() => _db.Close();
    }
}
