// Type definitions for jwt-decode v1.4.0
// Project: https://github.com/auth0/jwt-decode
// Definitions by: Giedrius Grabauskas <https://github.com/QuatroDevOfficial/>
// Definitions: https://github.com/DefinitelyTyped/DefinitelyTyped


declare namespace JwtDecode {
    interface JwtDecodeStatic {
        (token: string): any;
    }
}

// the original d.ts has a bug by using module 'jwt-decode'
declare module 'jwt_decode' {
    var jwtDecode: JwtDecode.JwtDecodeStatic;
    export = jwtDecode;
}
