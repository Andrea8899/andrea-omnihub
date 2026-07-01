// src/components/OmniHubLogo.tsx
export default function OmniHubLogo() {
  return (
    <svg 
      viewBox="0 0 100 100" 
      className="h-7 w-7 text-emerald-400 fill-none stroke-current shrink-0 transition-transform duration-300 hover:rotate-30"
      strokeWidth="6"
      strokeLinecap="round" 
      strokeLinejoin="round"
    >
      <polygon points="50,5 90,28 90,72 50,95 10,72 10,28" className="opacity-90" />
      <line x1="50" y1="5" x2="50" y2="95" strokeWidth="4" className="opacity-40" strokeDasharray="4 4" />
      <circle cx="50" cy="50" r="10" className="fill-[#0e1326] stroke-emerald-400" strokeWidth="5" />
    </svg>
  );
}