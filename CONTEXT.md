# BlogAPI

A blog backend exposing posts, tags, comments, and user profiles over a JSON API. Auth is JWT-based; reads are public, writes require an authenticated user.

## Language

**Post**:
A blog article written by a single **Post owner**, with a slug, content, and tags. Publicly readable.
_Avoid_: Article, entry

**Comment**:
A flat, top-level reply on a single **Post** by a **Comment author**. No nested replies.
_Avoid_: Reply, response, discussion

**Post owner**:
The **UserProfile** that authored a **Post**. Has implicit edit/delete rights over their own **Post**.
_Avoid_: Author (ambiguous — could mean comment), poster

**Comment author**:
The **UserProfile** that created a **Comment**. Has edit/delete rights over their own **Comment**.
_Avoid_: Commenter, replier

**UserProfile**:
The domain identity of a user — distinct from the framework-level `ApplicationUser` (Identity). Linked via `UserProfile.ApplicationUserId`. The thing that owns Posts and Comments.
_Avoid_: Account, user (overloaded)

**Tag**:
A keyword attached to a **Post** for discovery. Many-to-many with Post.

## Relationships

- A **Post** belongs to exactly one **Post owner** (UserProfile)
- A **Post** has zero or more **Comments**
- A **Comment** belongs to exactly one **Post** and exactly one **Comment author**
- Deleting a **Post** cascades to its **Comments**
- A **UserProfile** with existing **Comments** cannot be deleted (Restrict)

## Comment policy

- **Read**: public, no auth
- **Create**: auth-required; only an authenticated **UserProfile** may comment
- **Edit**: **Comment author** only, anytime, no time window
- **Delete**: **Comment author** only (admin moderation is the planned next step, deferred until role infrastructure exists; **Post owner** moderation was considered and rejected)
- **Lifecycle**: hard delete — no soft-delete flag, no audit trail
- **Threading**: flat — no nested replies

## Example dialogue

> **Dev:** "If a **Post owner** wants to remove a troll **Comment** on their own **Post**, can they?"
> **Domain expert:** "Not yet. Today only the **Comment author** can delete their own **Comment**. Admin moderation is the planned next step — it lands once we add a role system. We considered letting the **Post owner** moderate their own thread, but decided against it for now."

> **Dev:** "What happens to a **Post owner**'s **Comments** when their **Post** is deleted?"
> **Domain expert:** "All **Comments** on that **Post** are wiped via cascade. We don't keep orphaned discussion."

## Flagged ambiguities

- "Author" was used to mean both **Post owner** and **Comment author** — resolved: these are distinct concepts. The `AuthorDto` shape is shared but the role differs by context.
- "Account" / "User" were used loosely — resolved: **UserProfile** is the domain identity; `ApplicationUser` is the framework-level Identity record.
