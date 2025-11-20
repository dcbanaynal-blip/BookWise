// src/components/StatBadge.tsx
import { jsx, jsxs } from "react/jsx-runtime";
var toneMap = {
  primary: "bg-blue-100 text-blue-700",
  success: "bg-emerald-100 text-emerald-700",
  warning: "bg-amber-100 text-amber-700"
};
function StatBadge({ label, value, tone = "primary" }) {
  return /* @__PURE__ */ jsxs("div", { className: `inline-flex flex-col rounded-2xl px-4 py-3 ${toneMap[tone]}`, children: [
    /* @__PURE__ */ jsx("span", { className: "text-xs uppercase tracking-widest opacity-70", children: label }),
    /* @__PURE__ */ jsx("span", { className: "text-lg font-semibold", children: value })
  ] });
}
export {
  StatBadge
};
