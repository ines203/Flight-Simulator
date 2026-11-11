using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Database
{
    using SQLite;

    public class Utilisateur
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Pseudo { get; set; }
    }

    public class Statistiques
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int UtilisateurId { get; set; }

        public int MeilleurScore { get; set; }
        public int PartiesJouees { get; set; }
    }
}
