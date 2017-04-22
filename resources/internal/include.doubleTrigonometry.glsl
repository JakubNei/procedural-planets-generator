


// FROM: http://outerra.blogspot.cz/2014/05/double-precision-approximations-for-map.html
// atan2 approximation for doubles for GLSL
// using http://lolengine.net/wiki/doc/maths/remez

double atan_d(double y, double x)
{
    return atan(float(y), float(x));

    const double atan_tbl[] = {
    -3.333333333333333333333333333303396520128e-1LF,
     1.999999117496509842004185053319506031014e-1LF,
    -1.428514132711481940637283859690014415584e-1LF,
     1.110012236849539584126568416131750076191e-1LF,
    -8.993611617787817334566922323958104463948e-2LF,
     7.212338962134411520637759523226823838487e-2LF,
    -5.205055255952184339031830383744136009889e-2LF,
     2.938542391751121307313459297120064977888e-2LF,
    -1.079891788348568421355096111489189625479e-2LF,
     1.858552116405489677124095112269935093498e-3LF
    };

    /* argument reduction: 
       arctan (-x) = -arctan(x); 
       arctan (1/x) = 1/2 * pi - arctan (x), when x > 0
    */

    double ax = abs(x);
    double ay = abs(y);
    double t0 = max(ax, ay);
    double t1 = min(ax, ay);
    
    double a = 1 / t0;
    a *= t1;

    double s = a * a;
    double p = atan_tbl[9];

    p = fma( fma( fma( fma( fma( fma( fma( fma( fma( fma(p, s,
        atan_tbl[8]), s,
        atan_tbl[7]), s, 
        atan_tbl[6]), s,
        atan_tbl[5]), s,
        atan_tbl[4]), s,
        atan_tbl[3]), s,
        atan_tbl[2]), s,
        atan_tbl[1]), s,
        atan_tbl[0]), s*a, a);

    double r = ay > ax ? (1.57079632679489661923LF - p) : p;

    r = x < 0 ?  3.14159265358979323846LF - r : r;
    r = y < 0 ? -r : r;

    return r;
}








/* compute arcsin (a) for a in [-9/16, 9/16] */
double acos___helper (double a)
{
    double q, r, s, t;

    s = a * a;
    q = s * s;
    r =             5.5579749017470502e-2;
    t =            -6.2027913464120114e-2;
    r = fma (r, q,  5.4224464349245036e-2);
    t = fma (t, q, -1.1326992890324464e-2);
    r = fma (r, q,  1.5268872539397656e-2);
    t = fma (t, q,  1.0493798473372081e-2);
    r = fma (r, q,  1.4106045900607047e-2);
    t = fma (t, q,  1.7339776384962050e-2);
    r = fma (r, q,  2.2372961589651054e-2);
    t = fma (t, q,  3.0381912707941005e-2);
    r = fma (r, q,  4.4642857881094775e-2);
    t = fma (t, q,  7.4999999991367292e-2);
    r = fma (r, s, t);
    r = fma (r, s,  1.6666666666670193e-1);
    t = a * s;
    r = fma (r, t, a);

    return r;
}

/* Compute arccosine (a), maximum error observed: 1.4316 ulp
   Double-precision factorization of π courtesy of Tor Myklebust
*/
double acos_d(double a)
{
    return acos(float(a));

    double r;

    r = (a > 0.0) ? -a : a; // avoid modifying the "sign" of NaNs
    if (r > -0.5625) {
        /* arccos(x) = pi/2 - arcsin(x) */
        r = fma (9.3282184640716537e-1, 1.6839188885261840e+0, acos___helper (r));
    } else {
        /* arccos(x) = 2 * arcsin (sqrt ((1-x) / 2)) */
        r = 2.0 * acos___helper (sqrt (fma (0.5, r, 0.5)));
    }
    if (!(a > 0.0) && (a >= -1.0)) { // avoid modifying the "sign" of NaNs
        /* arccos (-x) = pi - arccos(x) */
        r = fma (1.8656436928143307e+0, 1.6839188885261840e+0, -r);
    }
    return r;
}


double asin_d(double x)
{
    const float pi = 3.1415926535897932384626433832795;
    return (pi/2.0 - acos_d(x));

}



double sin_d(double x)
{
    return sin(float(x));
}

double cos_d(double x)
{
    return cos(float(x));
}