


// FROM: http://outerra.blogspot.cz/2014/05/double-precision-approximations-for-map.html
// atan2 approximation for doubles for GLSL
// using http://lolengine.net/wiki/doc/maths/remez

double atan(double y, double x)
{
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




// FROM: http://stackoverflow.com/a/28988233/782022

double asin___helper(double x)
{
    double sum, tempExp;
    tempExp = x;
    double factor = 1.0;
    double divisor = 1.0;
    sum = x;
    for(int i = 0; i < 40; i++)
    {
        tempExp *= x*x;
        divisor += 2.0;
        factor *= (2.0*double(i) + 1.0)/((double(i)+1.0)*2.0);
        sum += factor*tempExp/divisor;
    }
    return sum;
}

double asin(double x)
{
    if(abs(x) <= 0.71)
        return asin___helper(x);
    else if( x > 0)
        return (PI/2.0-asin___helper(sqrt(1.0-(x*x))));
    else //x < 0 or x is NaN
        return (asin___helper(sqrt(1.0-(x*x)))-PI/2.0);

}

double acos(double x)
{
    return (PI/2.0 - asin(x));
}


