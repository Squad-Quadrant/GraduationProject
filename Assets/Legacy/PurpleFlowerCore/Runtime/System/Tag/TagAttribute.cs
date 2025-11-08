using UnityEngine;

namespace PurpleFlowerCore.Tag
{
    public class TagAttribute : PropertyAttribute
    {
        public string Catalogue { get; }
        public TagAttribute(string catalogue)
        {
            Catalogue = catalogue;
        }
        
        public TagAttribute()
        {
            Catalogue = null;
        }
    }
}