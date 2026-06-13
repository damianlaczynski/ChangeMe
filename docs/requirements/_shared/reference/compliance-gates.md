# Post-sign-in access

> Simplified authentication has no post-sign-in compliance gates.

After successful password validation on **Login** (FR-AUTH-001), the user receives a full application session and may access screens permitted by their effective permissions (FR-ROL-001).

There are no intermediate gates such as required password change, two-factor verification, passkey enrollment, email verification, or invitation acceptance.

## Sign-in evaluation

Password sign-in on **Login** is evaluated in this order:

1. Unknown credentials — fail with **`Invalid email or password.`** without revealing whether the email exists.
2. Account is **deactivated** — **`This account has been deactivated. Contact an administrator.`**
3. Success — create session and proceed to the application.

## Cross-references

- Login: FR-AUTH-001
- Deactivated accounts: FR-USR-005
- Initial administrator: FR-ROL-006
