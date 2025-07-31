using System.Diagnostics.CodeAnalysis;

namespace MeetMe.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities, providing core identity and equality functionality.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's identifier</typeparam>
    public abstract class BaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Unique identifier for the entity.
        /// </summary>
        public TKey Id { get; protected set; } = default!;

        /// <summary>
        /// Audit information - creation date
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Audit information - ID of the user who created this entity
        /// </summary>
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Audit information - last modification date
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Audit information - ID of the user who last modified this entity
        /// </summary>
        public string? LastModifiedByUserId { get; set; }

        /// <summary>
        /// Indicates if the entity is active (soft delete)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Updates the last modified auditing information  
        /// </summary>
        /// <param name="userId">ID of the user performing the update</param>
        protected void UpdateModifiedInfo(string userId)
        {
            LastModifiedDate = DateTime.UtcNow;
            LastModifiedByUserId = userId;
        }

        #region Equality and Comparison

        /// <summary>
        /// Equality comparison based on entity identity, not on entity reference
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            if (obj is not BaseEntity<TKey> other)
                return false;

            // Transient entities (not yet persisted) are never equal
            if (IsTransient() || other.IsTransient())
                return false;

            return Id.Equals(other.Id);
        }

        /// <summary>
        /// Determines if the entity is transient (not yet persisted to the database)
        /// </summary>
        public virtual bool IsTransient()
        {
            return Id == null || Id.Equals(default);
        }

        /// <summary>
        /// GetHashCode override based on entity ID
        /// </summary>
        public override int GetHashCode()
        {
            if (IsTransient())
                return base.GetHashCode();

            return Id.GetHashCode() ^ 31; // XOR for better hash distribution
        }

        /// <summary>
        /// Identity‐based equality; handles nulls and transients.  
        /// </summary>
        public static bool operator ==(BaseEntity<TKey>? left, BaseEntity<TKey>? right)
        {
            if (ReferenceEquals(left, right))      // both null or same ref
                return true;
            if (left is null || right is null)     // exactly one null
                return false;
            if (left.IsTransient() || right.IsTransient())
                return false;                      // never equal until persisted
            return left.Id.Equals(right.Id);       // real ID comparison
        }

        /// <summary>
        /// Negated identity test, but *also* tells the compiler:
        /// "If this returns true, then `left` is not null."
        /// </summary>
        public static bool operator !=(
            [NotNullWhen(true)] BaseEntity<TKey>? left,
            BaseEntity<TKey>? right)
        {
            return !(left == right);
        }

        #endregion
    }

    /// <summary>
    /// Convenience default implementation of Entity using int as the key type
    /// </summary>
    public abstract class BaseEntity : BaseEntity<int>
    {
    }
}
