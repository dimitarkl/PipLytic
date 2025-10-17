import { useEffect, useState } from "react";
import { CheckCircle2, X } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "../ui/alert";

type ToasterProps = {
  message: string;
  duration?: number;
  onClose?: () => void;
};

export default function Toaster({ message, duration = 4000, onClose }: ToasterProps) {
  const [visible, setVisible] = useState(false);
  const exitDuration = 220; 

  useEffect(() => {
    const t = requestAnimationFrame(() => setVisible(true));
    return () => {
      cancelAnimationFrame(t);
    };
  }, []);

  useEffect(() => {
    if (!duration) return;
    const timer = setTimeout(() => {
      setVisible(false);
      const finish = setTimeout(() => onClose?.(), exitDuration);
      return () => clearTimeout(finish);
    }, duration - exitDuration > 0 ? duration - exitDuration : duration);
    return () => clearTimeout(timer);
  }, [duration, onClose]);

  const handleClose = () => {
    setVisible(false);
    setTimeout(() => onClose?.(), exitDuration);
  };

  return (
    <Alert
      variant="default"
      className={`fixed left-1/2 -translate-x-1/2 top-15 z-50 max-w-lg w-auto px-4 flex items-start justify-between transition-all duration-200 ease-out ${
        visible
          ? "opacity-100 translate-y-0 scale-100"
          : "opacity-0 -translate-y-2 scale-95 pointer-events-none"
      }`}
      style={{ transitionProperty: "opacity, transform" }}
    >
      <div className="flex gap-2 items-start">
        <CheckCircle2 className="mt-0.5 text-green-500" />
        <div>
          <AlertTitle>Success</AlertTitle>
          <AlertDescription>{message}</AlertDescription>
        </div>
      </div>
      {onClose && (
        <button
          onClick={handleClose}
          className="ml-4 hover:opacity-75 text-sm text-muted-foreground"
        >
          <X size={16} />
        </button>
      )}
    </Alert>
  );
}
