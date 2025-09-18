
export type SpinnerProps = {
  size?: number; // in px
  className?: string;
  label?: string;
};

export default function Spinner({ size = 36, className = "", label }: SpinnerProps) {
  const stroke = Math.max(2, Math.round(size * 0.08));
  const viewBox = size;
  const radius = viewBox / 2 - stroke;
  const circumference = 2 * Math.PI * radius;

  return (
    <div className={`flex items-center gap-3 ${className}`}> 
      <div
        role="status"
        aria-label={label ?? "loading"}
        className="inline-flex items-center justify-center relative"
        style={{ width: size, height: size }}
      >
        <svg
          width={size}
          height={size}
          viewBox={`0 0 ${viewBox} ${viewBox}`}
          xmlns="http://www.w3.org/2000/svg"
          className="block animate-spin"
        >
          {/* Track */}
          <circle
            cx={viewBox / 2}
            cy={viewBox / 2}
            r={radius}
            strokeWidth={stroke}
            className="text-muted"
            stroke="currentColor"
            opacity={0.25}
            fill="none"
          />

          {/* Spinning arc */}
          <circle
            cx={viewBox / 2}
            cy={viewBox / 2}
            r={radius}
            strokeWidth={stroke}
            strokeLinecap="round"
            fill="none"
            className="text-primary"
            stroke="currentColor"
            strokeDasharray={circumference}
            strokeDashoffset={circumference * 0.75}
          />
        </svg>
      </div>

      {label ? (
        <span className="text-sm text-muted-foreground select-none">{label}</span>
      ) : null}
    </div>
  );
}