"use strict";
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

// src/index.ts
var index_exports = {};
__export(index_exports, {
  StatBadge: () => StatBadge
});
module.exports = __toCommonJS(index_exports);

// src/components/StatBadge.tsx
var import_jsx_runtime = require("react/jsx-runtime");
var toneMap = {
  primary: "bg-blue-100 text-blue-700",
  success: "bg-emerald-100 text-emerald-700",
  warning: "bg-amber-100 text-amber-700"
};
function StatBadge({ label, value, tone = "primary" }) {
  return /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("div", { className: `inline-flex flex-col rounded-2xl px-4 py-3 ${toneMap[tone]}`, children: [
    /* @__PURE__ */ (0, import_jsx_runtime.jsx)("span", { className: "text-xs uppercase tracking-widest opacity-70", children: label }),
    /* @__PURE__ */ (0, import_jsx_runtime.jsx)("span", { className: "text-lg font-semibold", children: value })
  ] });
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  StatBadge
});
