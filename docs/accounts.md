# Sovereign Engine User Accounts

## Account Privacy

> Sovereign Engine offers no protections for personally identifiable
> information that is voluntarily disclosed through the game, such as through
> in-game chat. It is the responsibility of the user to ensure that they do not
> release any information they wish to remain private.

> Sovereign Engine may store personally identifiable information such as client
> IP addresses in its server logs. It is the responsibility of the user to
> use a VPN to obscure their personal IP address if desired.

Sovereign Engine is designed to minimize the amount of personally identifiable
information required from the user. To this end, user accounts maintain the
minimum possible amount of information:

* Account ID, a randomly generated 128-bit UUID (not personally identifiable)
* Username, a user-specified case-sensitive string that publicly identifies
  the account
* Password salt, a 16-byte random sequence used for password security
* Password hash, a 16-byte sequence generated from the users' password and the
  password salt
* `opslimit`, an integer parameter not unique to any user
* `memlimit`, an integer parameter not unique to any user

Note that an email address is not associated with the account by default. It is
the responsibility of the server administrator to implement a policy for the
handling of lost passwords.

## Password Storage

Passwords are not stored in the Sovereign Engine database. Instead, one-way
hashes of user passwords are stored. Note that the plaintext password is still
held in memory by the server during authentication as it is required to compute
the hash for verification.

New passwords shall be hashed using the libsodium implementation of the Argon2 
password hashing algorithm (`crypto_pwhash` function) with the following 
parameters:

Parameter | Value
--- | ---
`alg` | `crypto_pwhash_ALG_ARGON2ID13`
`opslimit` | `crypto_pwhash_OPSLIMIT_INTERACTIVE`
`memlimit` | `crypto_pwhash_MEMLIMIT_INTERACTIVE`

The server shall store the values used for the `opslimit` and `memlimit`
parameters alongside the salt and hash in the database. The stored values of
these parameters shall be used when computing a password hash for comparison
during an authentication attempt.

